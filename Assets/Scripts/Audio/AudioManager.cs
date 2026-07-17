using UnityEngine;
using UnityEngine.Audio;

namespace DungeonEscape
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer Groups")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        [Header("Clips - BGM")]
        [SerializeField] private AudioClip defaultMusic;
        [SerializeField] private AudioClip victoryMusic;

        [Header("Clips - SFX")]
        [SerializeField] private AudioClip doorOpenSfx;
        [SerializeField] private AudioClip chestOpenSfx;

        private AudioSource bgmSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAudioSources();
        }

        private void OnEnable()
        {
            DungeonEvents.OnDoorUnlocked += PlayDoorSfx;
            DungeonEvents.OnChestOpened += PlayChestSfx;
            DungeonEvents.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDisable()
        {
            DungeonEvents.OnDoorUnlocked -= PlayDoorSfx;
            DungeonEvents.OnChestOpened -= PlayChestSfx;
            DungeonEvents.OnGameStateChanged -= HandleGameStateChanged;
        }

        private void InitializeAudioSources()
        {
            // BGM Source
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            if (musicMixerGroup != null)
            {
                bgmSource.outputAudioMixerGroup = musicMixerGroup;
            }

            // SFX Source
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            if (sfxMixerGroup != null)
            {
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
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
                    PlayBGM(defaultMusic);
                    break;
                case GameState.Paused:
                    break;
                case GameState.GameOver:
                    StopBGM();
                    break;
                case GameState.Victory:
                    PlayBGM(victoryMusic);
                    break;
            }
        }

        public void PlayBGM(AudioClip clip)
        {
            if (bgmSource == null || clip == null) return;

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip);
        }

        public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position);
        }

        private void PlayDoorSfx()
        {
            DungeonDoor door = FindFirstObjectByType<DungeonDoor>();
            if (door != null)
            {
                PlaySFXAtPosition(doorOpenSfx, door.transform.position);
            }
            else
            {
                PlaySFX(doorOpenSfx);
            }
        }

        private void PlayChestSfx()
        {
            DungeonChest chest = FindFirstObjectByType<DungeonChest>();
            if (chest != null)
            {
                PlaySFXAtPosition(chestOpenSfx, chest.transform.position);
            }
            else
            {
                PlaySFX(chestOpenSfx);
            }
        }
    }
}
