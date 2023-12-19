
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace VowganVR
{
    public class VVRMusicPlayer : UdonSharpBehaviour
    {
        
        public float postSongWait = 1;
        public int songIndex;
        [Range(0, 3)] public int playbackType;
        public bool playOnStart;
        
        [Header("Songs")]
        public Sprite[] songIcons;
        public AudioClip[] songClips;
        public string[] songTitles;
        public string[] songArtists;
        
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
        
        private int lastFramerate;
        private float[] audioSpectrum = new float[256];
        private float songTime;
        private float queueCache;
        private int state;
        private int stateOld;
        private int findNonNullSongChecks;
        private bool isPlaying;
        private bool isPaused;
        
        void Start()
        {
            _UpdateBannerTopRight();
            _SetVolume();
            _UnMute();
            if (playOnStart) LoadNewSong();

            switch (playbackType)
            {
                case 0:
                    _PlaybackNormal();
                    break;
                case 1:
                    _PlaybackRepeatAll();
                    break;
                case 2:
                    _PlaybackRepeatOne();
                    break;
                case 3:
                    _PlaybackShuffle();
                    break;
            }
        }
        
        private void Update()
        {
            switch (state)
            {
                case 0: // Idle
                    if (stateOld != state)
                    {
                        stateOld = state;
                        isPaused = false;
                        isPlaying = false;
                        pauseButton.SetActive(false);
                        playButton.SetActive(true);
                    }
                    break;
                
                case 1: // Playing
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
                        state = 2;
                        isPaused = false;
                        isPlaying = false;
                    }
                    else
                    {
                        UpdateVisuals();
                    }
                    break;
                
                case 2: // Post-Song
                    if (stateOld != state)
                    {
                        stateOld = state;
                    }
                    if (!isPaused) queueCache += Time.deltaTime;
                    if (queueCache > postSongWait)
                    {
                        queueCache = 0;
                        LoadNext();
                    }
                    break;
            }
        }

        public void _LoadState0() => state = 0;
        public void _LoadState1() => state = 1;
        public void _LoadState2() => state = 2;
        
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
            switch (songClips.Length)
            {
                case 0:
                    state = 0;
                    break;
                case 1:
                    if (songClips[0] == null)
                    {
                        state = 0;
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
        
        private void LoadNext()
        {
            switch (playbackType)
            {
                case 0:
                    songIndex += 1;
                    if (songIndex < songClips.Length)
                    {
                        LoadNewSong();
                    }
                    else
                    {
                        state = 0;
                    }
                    break;
                
                case 1:
                    if (songIndex >= songClips.Length)
                    {
                        LoadNewSong();
                    }
                    else
                    {
                        songIndex += 1;
                        LoadNewSong();
                    }
                    break;
                
                case 2:
                    LoadNewSong();
                    break;
                
                case 3:
                    var songOld = songIndex;
                    songIndex = UnityEngine.Random.Range(0, songClips.Length - 1);
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
            if (songIndex < 0) songIndex = songClips.Length - 1;
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
            playbackType = 0;
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
            playbackType = 1;
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
            playbackType = 2;
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
            playbackType = 3;
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
            if (songIndex >= songClips.Length) songIndex = 0;
            if (songIndex != -1)
            {
                state = 1;
                if (songClips[songIndex] != null)
                {
                    source.clip = songClips[songIndex];
                    previewImage.sprite = songIcons[songIndex];
                    textTitle.text = songTitles[songIndex];
                    textArtist.text = songArtists[songIndex];
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
                        state = 0;
                    }
                    else
                    {
                        _PlayNext();
                    }
                }
            }
            else
            {
                state = 0;
            }
        }
        
    }
}