using UnityEngine;
using UnityEngine.Audio;

namespace DungeonEscape
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips - BGM")]
        [SerializeField] private AudioClip dungeonMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip gameOverMusic;

        [Header("Clips - SFX")]
        [SerializeField] private AudioClip keyPickupSfx;
        [SerializeField] private AudioClip doorOpenSfx;
        [SerializeField] private AudioClip chestOpenSfx;
        [SerializeField] private AudioClip playerHitSfx;
        [SerializeField] private AudioClip spikeTrapSfx;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Asegurarse de que existan AudioSources si no se asignaron
            InitializeAudioSources();
        }

        private void OnEnable()
        {
            DungeonEvents.OnKeyCollected += PlayKeySfx;
            DungeonEvents.OnDoorUnlocked += PlayDoorSfx;
            DungeonEvents.OnPlayerDamage += PlayDamageSfx;
            DungeonEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            DungeonEvents.OnKeyCollected -= PlayKeySfx;
            DungeonEvents.OnDoorUnlocked -= PlayDoorSfx;
            DungeonEvents.OnPlayerDamage -= PlayDamageSfx;
            DungeonEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                if (musicMixerGroup != null) bgmSource.outputAudioMixerGroup = musicMixerGroup;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                if (sfxMixerGroup != null) sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    StopBGM();
                    break;
                case GameState.Playing:
                    PlayBGM(dungeonMusic);
                    break;
                case GameState.Paused:
                    // Podríamos bajar el volumen o pausar la música
                    break;
                case GameState.GameOver:
                    PlayBGM(gameOverMusic);
                    break;
                case GameState.Victory:
                    PlayBGM(victoryMusic);
                    break;
            }
        }

        private void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;

            // Evitar reiniciar el mismo clip que ya está sonando
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        private void StopBGM()
        {
            if (bgmSource != null) bgmSource.Stop();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            // Para audio 3D posicional
            AudioSource.PlayClipAtPoint(clip, position);
        }

        private void PlayKeySfx(int amount)
        {
            if (amount > 0) // No reproducir en actualizaciones indirectas (0)
            {
                PlaySFX(keyPickupSfx);
            }
        }

        private void PlayDoorSfx()
        {
            PlaySFX(doorOpenSfx);
        }

        private void PlayDamageSfx(int currentHealth)
        {
            // Solo reproducir si sigue vivo (si muere, suena la música de Game Over)
            if (currentHealth > 0)
            {
                PlaySFX(playerHitSfx);
            }
        }

        // Método público por si las trampas u otros scripts quieren disparar SFX directamente
        public void PlaySpikeTrapSfx(Vector3 pos)
        {
            PlaySFXAtPosition(spikeTrapSfx, pos);
        }
    }
}
