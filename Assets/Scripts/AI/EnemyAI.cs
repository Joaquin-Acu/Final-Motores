using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

namespace DungeonEscape
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        public enum EnemyState { Patrolling, Investigating, Chasing, Attacking }

        [Header("AI Settings")]
        [SerializeField] private EnemyState currentState = EnemyState.Patrolling;
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float chaseSpeed = 4f;

        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 8f;
        [SerializeField] private float fovAngle = 90f;
        [SerializeField] private LayerMask playerMask;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Hearing Settings")]
        [SerializeField] private float hearingRadius = 6f; // Rango de distancia para escuchar los pasos del jugador
        [SerializeField] private float investigationDuration = 3f; // Tiempo que pasa buscando en el lugar del sonido

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolWaypoints;
        [SerializeField] private float waypointTolerance = 0.5f;
        [SerializeField] private float randomPatrolRadius = 10f; // Usado si no hay waypoints

        [Header("Attack Settings")]
        [SerializeField] private int attackDamage = 20;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamageDelay = 0.5f; // Retardo en segundos para sincronizar el impacto con el golpe visual de la animación

        [Header("Animations")]
        [SerializeField] private Animator animator;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource enemyAudioSource; // AudioSource local en el enemigo (3D)
        [SerializeField] private AudioMixerGroup sfxGroup; // Enrutar al canal de SFX
        [SerializeField] private AudioClip growlSfx; // Sonido de gruñido del zombie (wav)

        private NavMeshAgent agent;
        private Transform playerTransform;
        private PlayerHealth playerHealth;
        private PlayerController playerController;

        private Vector3 targetPatrolPoint;
        private int currentWaypointIndex = 0;
        private float lastAttackTime = 0f;
        private bool isPlayerVisible = false;

        // Variables de investigación auditiva
        private Vector3 investigationPoint;
        private float investigationTimer = 0f;
        private bool isInvestigatingPointReached = false;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = walkSpeed;

            // Inicializar AudioSource del enemigo si no está asignado
            if (enemyAudioSource == null)
            {
                enemyAudioSource = gameObject.AddComponent<AudioSource>();
                enemyAudioSource.playOnAwake = false;
                enemyAudioSource.loop = false;
                enemyAudioSource.spatialBlend = 1.0f; // Audio 3D (para escuchar la posición física del zombie)
                enemyAudioSource.minDistance = 2f;
                enemyAudioSource.maxDistance = 15f;
            }

            if (sfxGroup != null)
            {
                enemyAudioSource.outputAudioMixerGroup = sfxGroup;
            }
        }

        private void Start()
        {
            // Intentar encontrar al jugador automáticamente
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<PlayerHealth>();
                playerController = player.GetComponent<PlayerController>();
            }

            SetNextPatrolPoint();
        }

        private void Update()
        {
            if (playerTransform == null) return;

            CheckPlayerVisibility();

            // Máquina de estados
            switch (currentState)
            {
                case EnemyState.Patrolling:
                    PatrolBehavior();
                    break;
                case EnemyState.Investigating:
                    InvestigateBehavior();
                    break;
                case EnemyState.Chasing:
                    ChaseBehavior();
                    break;
                case EnemyState.Attacking:
                    AttackBehavior();
                    break;
            }

            UpdateAnimator();
        }

        private void CheckPlayerVisibility()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= detectionRadius)
            {
                Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToPlayer);

                // Si está dentro del cono visual (FOV)
                if (angle < fovAngle / 2f)
                {
                    // Lanzar Raycast para verificar que no haya paredes en medio
                    if (!Physics.Raycast(transform.position + Vector3.up * 1f, directionToPlayer, distanceToPlayer, obstacleMask))
                    {
                        isPlayerVisible = true;
                        return;
                    }
                }
            }

            isPlayerVisible = false;
        }

        private void CheckHearing()
        {
            if (playerTransform == null || playerController == null) return;

            // Solo escuchar si el jugador está haciendo ruido de pisadas
            if (playerController.IsMakingNoise)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= hearingRadius)
                {
                    // La audición interrumpe patrulla o redirecciona investigación actual
                    if (currentState == EnemyState.Patrolling || currentState == EnemyState.Investigating)
                    {
                        investigationPoint = playerTransform.position;
                        isInvestigatingPointReached = false;
                        investigationTimer = 0f;
                        SetState(EnemyState.Investigating);
                    }
                }
            }
        }

        private void SetState(EnemyState newState)
        {
            if (currentState == newState) return;

            EnemyState oldState = currentState;
            currentState = newState;

            // Si pasa a perseguir o investigar desde patrulla, reproducir el rugido del zombie
            if (newState == EnemyState.Chasing && oldState != EnemyState.Chasing)
            {
                PlayChaseGrowl();
            }
            else if (newState == EnemyState.Investigating && oldState == EnemyState.Patrolling)
            {
                PlayChaseGrowl();
            }
        }

        private void PlayChaseGrowl()
        {
            if (enemyAudioSource != null && growlSfx != null)
            {
                enemyAudioSource.PlayOneShot(growlSfx);
            }
        }

        private void PatrolBehavior()
        {
            if (!agent.isOnNavMesh) return;

            agent.speed = walkSpeed;

            // 1. La vista tiene prioridad absoluta
            if (isPlayerVisible)
            {
                SetState(EnemyState.Chasing);
                return;
            }

            // 2. Si no lo ve, ver si lo escucha
            CheckHearing();
            if (currentState == EnemyState.Investigating) return;

            // Moverse al punto de patrulla
            if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
            {
                SetNextPatrolPoint();
            }
        }

        private void InvestigateBehavior()
        {
            if (!agent.isOnNavMesh) return;

            // 1. La vista tiene prioridad absoluta (si lo ve, persigue de una)
            if (isPlayerVisible)
            {
                SetState(EnemyState.Chasing);
                return;
            }

            // 2. Seguir escuchando al jugador mientras investiga (para actualizar su punto de sospecha)
            CheckHearing();

            agent.speed = walkSpeed * 1.2f; // Camina un poco más rápido y tenso al investigar
            agent.destination = investigationPoint;

            // Comprobar si llegó a la posición del ruido
            if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
            {
                isInvestigatingPointReached = true;
            }

            if (isInvestigatingPointReached)
            {
                agent.speed = 0f; // Detenerse en el punto de sospecha
                
                // Esperar buscando a su alrededor
                investigationTimer += Time.deltaTime;
                if (investigationTimer >= investigationDuration)
                {
                    // Volver a patrullar pacíficamente
                    SetState(EnemyState.Patrolling);
                    SetNextPatrolPoint();
                }
            }
        }

        private void SetNextPatrolPoint()
        {
            if (!agent.isOnNavMesh) return;

            if (patrolWaypoints != null && patrolWaypoints.Length > 0)
            {
                agent.destination = patrolWaypoints[currentWaypointIndex].position;
                currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
            }
            else
            {
                // Patrulla aleatoria en NavMesh si no hay waypoints manuales
                Vector3 randomDirection = Random.insideUnitSphere * randomPatrolRadius;
                randomDirection += transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, randomPatrolRadius, 1))
                {
                    agent.destination = hit.position;
                }
            }
        }

        private void ChaseBehavior()
        {
            if (!agent.isOnNavMesh) return;

            agent.speed = chaseSpeed;
            agent.destination = playerTransform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Si se acerca a rango de ataque
            if (distanceToPlayer <= attackRange)
            {
                SetState(EnemyState.Attacking);
                return;
            }

            // Si pierde de vista al jugador a cierta distancia, vuelve a patrullar
            if (!isPlayerVisible && distanceToPlayer > detectionRadius)
            {
                SetState(EnemyState.Patrolling);
                SetNextPatrolPoint();
            }
        }

        private void AttackBehavior()
        {
            if (!agent.isOnNavMesh) return;

            agent.speed = 0f; // Detenerse mientras ataca
            agent.destination = transform.position;

            // Rotar hacia el jugador suavemente
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0; // Evitar inclinación
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Si el jugador se aleja
            if (distanceToPlayer > attackRange)
            {
                SetState(EnemyState.Chasing);
                return;
            }

            // Atacar
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                ExecuteAttack();
            }
        }

        private void ExecuteAttack()
        {
            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }

            if (enemyAudioSource != null && growlSfx != null)
            {
                enemyAudioSource.PlayOneShot(growlSfx);
            }

            StartCoroutine(DamageDelayCoroutine());
        }

        private IEnumerator DamageDelayCoroutine()
        {
            yield return new WaitForSeconds(attackDamageDelay);

            // Verificar que el jugador siga a rango de golpe tras el retraso de la animación
            if (playerTransform != null && playerHealth != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                // Damos un margen extra de 0.5m por si el jugador se está moviendo hacia atrás
                if (distanceToPlayer <= attackRange + 0.5f)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                // Enviar la velocidad actual para animaciones de caminar/correr
                animator.SetFloat("Speed", agent.velocity.magnitude);
                // Activar modo agresivo en animaciones si está persiguiendo
                animator.SetBool("IsChasing", currentState == EnemyState.Chasing);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Dibujar radio de detección en azul
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            // Dibujar rango de audición en verde (así lo ve visualmente el diseñador en el editor!)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);

            // Dibujar rango de ataque en rojo
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // Dibujar cono de visión en amarillo
            Gizmos.color = Color.yellow;
            Vector3 fovLeft = Quaternion.AngleAxis(-fovAngle / 2f, Vector3.up) * transform.forward;
            Vector3 fovRight = Quaternion.AngleAxis(fovAngle / 2f, Vector3.up) * transform.forward;
            Gizmos.DrawRay(transform.position, fovLeft * detectionRadius);
            Gizmos.DrawRay(transform.position, fovRight * detectionRadius);
        }
    }
}
