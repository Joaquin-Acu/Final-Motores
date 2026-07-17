using System.Collections;
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
        [SerializeField] private LayerMask obstacleMask;

        [Header("Hearing Settings")]
        [SerializeField] private float hearingRadius = 6f;
        [SerializeField] private float investigationDuration = 3f;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] patrolWaypoints;
        [SerializeField] private float waypointTolerance = 0.5f;
        [SerializeField] private float randomPatrolRadius = 10f;

        [Header("Attack Settings")]
        [SerializeField] private int attackDamage = 20;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackDamageDelay = 0.5f;

        [Header("Animations")]
        [SerializeField] private Animator animator;

        [Header("Audio Settings")]
        [SerializeField] private AudioSource enemyAudioSource;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioClip growlSfx;
        [SerializeField] private AudioClip hitSfx;
        [SerializeField] private float growlCooldown = 3.0f;

        private NavMeshAgent agent;
        private Transform playerTransform;
        private PlayerHealth playerHealth;
        private PlayerController playerController;

        private Vector3 targetPatrolPoint;
        private int currentWaypointIndex = 0;
        private float lastAttackTime = 0f;
        private bool isPlayerVisible = false;

        private Vector3 investigationPoint;
        private float investigationTimer = 0f;
        private bool isInvestigatingPointReached = false;
        private float lastGrowlTime = -99f;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = walkSpeed;

            if (enemyAudioSource == null)
            {
                enemyAudioSource = gameObject.AddComponent<AudioSource>();
                enemyAudioSource.playOnAwake = false;
                enemyAudioSource.loop = false;
                enemyAudioSource.spatialBlend = 1.0f;
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

                if (angle < fovAngle / 2f)
                {
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

            if (playerController.IsMakingNoise)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= hearingRadius)
                {
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

            if (newState == EnemyState.Chasing && oldState != EnemyState.Chasing)
            {
                TryPlayGrowl();
            }
            else if (newState == EnemyState.Investigating && oldState == EnemyState.Patrolling)
            {
                TryPlayGrowl();
            }
        }

        private void TryPlayGrowl()
        {
            if (Time.time - lastGrowlTime >= growlCooldown)
            {
                if (enemyAudioSource != null && growlSfx != null)
                {
                    enemyAudioSource.PlayOneShot(growlSfx);
                    lastGrowlTime = Time.time;
                }
            }
        }

        private void PatrolBehavior()
        {
            if (!agent.isOnNavMesh) return;

            agent.speed = walkSpeed;

            if (isPlayerVisible)
            {
                SetState(EnemyState.Chasing);
                return;
            }

            CheckHearing();
            if (currentState == EnemyState.Investigating) return;

            if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
            {
                SetNextPatrolPoint();
            }
        }

        private void InvestigateBehavior()
        {
            if (!agent.isOnNavMesh) return;

            if (isPlayerVisible)
            {
                SetState(EnemyState.Chasing);
                return;
            }

            CheckHearing();

            agent.speed = walkSpeed * 1.2f;
            agent.destination = investigationPoint;

            if (!agent.pathPending && agent.remainingDistance < waypointTolerance)
            {
                isInvestigatingPointReached = true;
            }

            if (isInvestigatingPointReached)
            {
                agent.speed = 0f;
                investigationTimer += Time.deltaTime;
                if (investigationTimer >= investigationDuration)
                {
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

            if (distanceToPlayer <= attackRange)
            {
                SetState(EnemyState.Attacking);
                return;
            }

            if (!isPlayerVisible && distanceToPlayer > detectionRadius)
            {
                SetState(EnemyState.Patrolling);
                SetNextPatrolPoint();
            }
        }

        private void AttackBehavior()
        {
            if (!agent.isOnNavMesh) return;

            agent.speed = 0f;
            agent.destination = transform.position;

            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer > attackRange)
            {
                SetState(EnemyState.Chasing);
                return;
            }

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

            TryPlayGrowl();
            StartCoroutine(DamageDelayCoroutine());
        }

        private IEnumerator DamageDelayCoroutine()
        {
            yield return new WaitForSeconds(attackDamageDelay);

            if (playerTransform != null && playerHealth != null)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= attackRange + 0.5f)
                {
                    playerHealth.TakeDamage(attackDamage);

                    if (enemyAudioSource != null && hitSfx != null)
                    {
                        enemyAudioSource.PlayOneShot(hitSfx);
                    }
                }
            }
        }

        private void UpdateAnimator()
        {
            if (animator != null)
            {
                animator.SetFloat("Speed", agent.velocity.magnitude);
                animator.SetBool("IsChasing", currentState == EnemyState.Chasing);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
