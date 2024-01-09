
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace Vowgan.Music
{

    public enum MusicPlayerState
    {
        Idle,
        Playing,
        PostSong,
    }
    
    public enum PlaybackType
    {
        Normal,
        RepeatAll,
        RepeatOne,
        Shuffle,
    }
    
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicPlayer : UdonSharpBehaviour
    {
        
        public PlaybackType PlaybackType;
        [MusicPlaylist] public UnityEngine.Object Playlist;
        public float PostSongWait = 1;
        public bool PlayOnStart;
        
        public string[] SongNames;
        public string[] SongArtists;
        public Sprite[] SongIcons;
        public AudioClip[] SongClips;
        
        [Header("References")]
        [Range(0.01f, 1f)] public float visualizerSmoothing = 0.35f;
        public AudioSource source;
        public Image wheelProgress;
        public Image previewImage;
        public TextMeshProUGUI textTime;
        public TextMeshProUGUI textTitle;
        public TextMeshProUGUI textArtist;
        public Slider volumeSlider;
        public Transform[] pulseVisuals;
        public GameObject playButton;
        public GameObject pauseButton;
        public GameObject repeatOffButton;
        public GameObject repeatOnButton;
        public GameObject repeatOneButton;
        public GameObject shuffleButton;
        public GameObject muteButton;
        public GameObject unmuteButton;

        private int songIndex = 0;
        private float[] audioSpectrum = new float[256];
        private float songTime;
        [SerializeField] private MusicPlayerState state = MusicPlayerState.Idle;
        [SerializeField] private MusicPlayerState stateOld = MusicPlayerState.Idle;
        private int findNonNullSongChecks;
        private bool isPlaying;
        private bool isPaused;


        private void Start()
        {
            _UpdateBannerTopRight();
            _SetVolume();
            _UnMute();
            if (PlayOnStart) LoadNewSong();

            switch (PlaybackType)
            {
                case PlaybackType.Normal:
                    _PlaybackNormal();
                    break;
                case PlaybackType.RepeatAll:
                    _PlaybackRepeatAll();
                    break;
                case PlaybackType.RepeatOne:
                    _PlaybackRepeatOne();
                    break;
                case PlaybackType.Shuffle:
                    _PlaybackShuffle();
                    break;
            }
        }
        
        private void Update()
        {
            switch (state)
            {
                case MusicPlayerState.Idle:
                    if (stateOld != state)
                    {
                        stateOld = state;
                        isPaused = false;
                        isPlaying = false;
                        pauseButton.SetActive(false);
                        playButton.SetActive(true);
                    }
                    break;
                
                case MusicPlayerState.Playing:
                    if (stateOld != state)
                    {
                        stateOld = state;
                        LoadNewSong();
                    }
                    
                    if (isPaused)
                    {
                        
                    }
                    else if (!source.isPlaying)
                    {
                        state = MusicPlayerState.PostSong;
                        isPaused = false;
                        isPlaying = false;
                    }
                    else
                    {
                        UpdateVisuals();
                    }
                    break;
                
                case MusicPlayerState.PostSong:
                    if (stateOld != state)
                    {
                        stateOld = state;
                        SendCustomEventDelayedSeconds(nameof(_LoadNext), PostSongWait);
                    }
                    break;
            }
        }

        public void _LoadState0() => state = MusicPlayerState.Idle;
        public void _LoadState1() => state = MusicPlayerState.Playing;
        public void _LoadState2() => state = MusicPlayerState.PostSong;
        
        public void _UpdateBannerTopRight()
        {
            var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            textTime.text = $"{currentTime:hh}:{currentTime.Minute} {currentTime:tt}";
            SendCustomEventDelayedSeconds(nameof(_UpdateBannerTopRight), 60);
        }
        
        private void UpdateVisuals()
        {
            songTime = source.time / source.clip.length;
            wheelProgress.fillAmount = Mathf.Lerp(0.04f, 0.97f, songTime);
            
            source.GetSpectrumData(audioSpectrum, 0, FFTWindow.Rectangular);
            for (int i = 0; i < pulseVisuals.Length; i++)
            {
                pulseVisuals[i].localScale = Vector3.Lerp(
                    pulseVisuals[i].localScale,
                    Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, audioSpectrum[i + 1] * (i + 3)),
                    visualizerSmoothing);
            }
        }

        /// <summary>
        /// Plays the next song. Must be public so UI Buttons can access it.
        /// </summary>
        public void _PlayNext()
        {
            switch (SongClips.Length)
            {
                case 0:
                    state = MusicPlayerState.Idle;
                    break;
                case 1:
                    if (SongClips[0] == null)
                    {
                        state = MusicPlayerState.Idle;
                    }
                    else
                    {
                        songIndex += 1;
                        LoadNewSong();
                    }
                    break;
                default:
                    songIndex += 1;
                    LoadNewSong();
                    break;
            }
        }
        
        public void _LoadNext()
        {
            switch (PlaybackType)
            {
                case PlaybackType.Normal:
                    songIndex += 1;
                    if (songIndex < SongClips.Length)
                    {
                        LoadNewSong();
                    }
                    else
                    {
                        state = MusicPlayerState.Idle;
                    }
                    break;
                
                case PlaybackType.RepeatAll:
                    if (songIndex >= SongClips.Length)
                    {
                        LoadNewSong();
                    }
                    else
                    {
                        songIndex += 1;
                        LoadNewSong();
                    }
                    break;
                
                case PlaybackType.RepeatOne:
                    LoadNewSong();
                    break;
                
                case PlaybackType.Shuffle:
                    var songOld = songIndex;
                    songIndex = UnityEngine.Random.Range(0, SongClips.Length - 1);
                    if (songIndex == songOld) songIndex++;
                    LoadNewSong();
                    break;
            }
        }
        
        /// <summary>
        /// Plays the previous song. Must be public for UI buttons.
        /// </summary>
        public void _PlayLast()
        {
            songIndex -= 1;
            if (songIndex < 0) songIndex = SongClips.Length - 1;
            LoadNewSong();
        }
        
        /// <summary>
        /// Pauses the current playback. Must be public for UI buttons.
        /// </summary>
        public void _Pause()
        {
            source.Pause();
            isPaused = true;
            pauseButton.SetActive(false);
            playButton.SetActive(true);
        }
        
        /// <summary>
        /// Resumes the current playback. Must be public for UI buttons.
        /// </summary>
        public void _Resume()
        {
            source.UnPause();
            if (!isPaused)
            {
                LoadNewSong();
            }
            isPaused = false;
            pauseButton.SetActive(true);
            playButton.SetActive(false);
        }

        /// <summary>
        /// Uses normal playback mode, so no repeats or shuffling. Must be public for UI buttons.
        /// </summary>
        public void _PlaybackNormal()
        {
            PlaybackType = PlaybackType.Normal;
            repeatOffButton.SetActive(true);
            repeatOnButton.SetActive(false);
            repeatOneButton.SetActive(false);
            shuffleButton.SetActive(false);
        }

        /// <summary>
        /// Uses the Repeat All playback mode. Must be public for UI buttons.
        /// </summary>
        public void _PlaybackRepeatAll()
        {
            PlaybackType = PlaybackType.RepeatAll;
            repeatOffButton.SetActive(false);
            repeatOnButton.SetActive(true);
            repeatOneButton.SetActive(false);
            shuffleButton.SetActive(false);
        }
        /// <summary>
        /// Uses the Repeat One playback mode. Must be public for UI buttons.
        /// </summary>
        public void _PlaybackRepeatOne()
        {
            PlaybackType = PlaybackType.RepeatOne;
            repeatOffButton.SetActive(false);
            repeatOnButton.SetActive(false);
            repeatOneButton.SetActive(true);
            shuffleButton.SetActive(false);
        }
        /// <summary>
        /// Uses the Shuffle playback mode. Must be public for UI buttons.
        /// </summary>
        public void _PlaybackShuffle()
        {
            PlaybackType = PlaybackType.Shuffle;
            repeatOffButton.SetActive(false);
            repeatOnButton.SetActive(false);
            repeatOneButton.SetActive(false);
            shuffleButton.SetActive(true);
        }

        /// <summary>
        /// Sets the audiosource volume to the value of the volume slider. Must be public for UI buttons.
        /// </summary>
        public void _SetVolume()
        {
            source.volume = volumeSlider.value;
        }

        /// <summary>
        /// Mutes the current audio, but continues playback. Must be public for UI buttons.
        /// </summary>
        public void _Mute()
        {
            source.mute = true;
            muteButton.SetActive(false);
            unmuteButton.SetActive(true);
        }

        /// <summary>
        /// UnMutes the current audio and continues playback. Must be public for UI buttons.
        /// </summary>
        public void _UnMute()
        {
            source.mute = false;
            muteButton.SetActive(true);
            unmuteButton.SetActive(false);
        }
        
        private void LoadNewSong()
        {
            if (songIndex >= SongClips.Length) songIndex = 0;
            if (songIndex != -1)
            {
                state = MusicPlayerState.PostSong;
                if (SongClips[songIndex] != null)
                {
                    source.clip = SongClips[songIndex];
                    previewImage.sprite = SongIcons[songIndex];
                    textTitle.text = SongNames[songIndex];
                    textArtist.text = SongArtists[songIndex];
                    source.Play();
                    pauseButton.SetActive(true);
                    playButton.SetActive(false);
                    isPaused = false;
                    isPlaying = true;
                }
                else
                {
                    findNonNullSongChecks += 1;
                    if (findNonNullSongChecks >= 10)
                    {
                        findNonNullSongChecks = 0;
                        state = MusicPlayerState.Idle;
                    }
                    else
                    {
                        _PlayNext();
                    }
                }
            }
            else
            {
                state = MusicPlayerState.Idle;
            }
        }
        
    }
}