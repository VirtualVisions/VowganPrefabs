
using System;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Vowgan.Music
{

    public enum MusicPlaybackType
    {
        Normal,
        RepeatAll,
        RepeatOne,
        Shuffle,
    }

    public enum MusicPlayerState
    {
        Idle,
        Playing,
        PostSong,
    }
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class MusicPlayer : UdonSharpBehaviour
    {
        
        [Header("Options")]
        [MusicPlaylist] public UnityEngine.Object Playlist;
        public MusicPlaybackType PlaybackType;
        public bool PlayOnStart;
        public float PostSongWait = 1;
        public float VisualizerMultiplier = 1.5f;
        [Range(0.01f, 1f)] public float VisualizerSmoothing = 0.35f;

        [HideInInspector] public string[] SongNames;
        [HideInInspector] public string[] SongArtists;
        [HideInInspector] public AudioClip[] SongClips;
        [HideInInspector] public Sprite[] SongIcons;
        
        [Header("Readout")]
        [SerializeField] private MusicPlayerState PlayerState;
        [SerializeField] private bool Paused;
        [SerializeField] private bool Muted;
        [SerializeField] private int SongIndex;
        [SerializeField] private float StateTimer;
        [SerializeField] private float LeaveStateTime;
        
        [Header("References")]
        [SerializeField] private AudioSource Source;
        [SerializeField] private  Transform[] PulseVisuals;
        [SerializeField] private  Image WheelProgress;
        [SerializeField] private  GameObject ButtonPlay;
        [SerializeField] private  GameObject ButtonPause;
        [SerializeField] private  GameObject ButtonRepeat;
        [SerializeField] private  GameObject ButtonRepeatAll;
        [SerializeField] private  GameObject ButtonRepeat1;
        [SerializeField] private  GameObject ButtonShuffle;
        [SerializeField] private  GameObject ButtonVolumeMute;
        [SerializeField] private  GameObject ButtonVolumeUnmute;
        [SerializeField] private  Image VolumeMute;
        [SerializeField] private  Slider SliderVolume;
        [SerializeField] private  Sprite Volume0;
        [SerializeField] private  Sprite Volume1;
        [SerializeField] private  Sprite Volume2;
        [SerializeField] private  Image ImageIcon;
        [SerializeField] private  TextMeshProUGUI TextSongName;
        [SerializeField] private  TextMeshProUGUI TextArtistName;
        
        
        private int songCount;
        private readonly float[] audioSpectrum = new float[256];


        private void Start()
        {
            songCount = SongClips.Length;

            SetPlayType(MusicPlaybackType.Normal);
            
            if (PlayOnStart)
            {
                _Play();
            }
        }

        private void Update()
        {
            switch (PlayerState)
            {
                case MusicPlayerState.Idle:
                    break;
                case MusicPlayerState.Playing:
                    if (Paused) break;
                    UpdateVisuals();
                    if (CheckTimer())
                    {
                        SetPlayerState(MusicPlayerState.PostSong);
                    }
                    break;
                case MusicPlayerState.PostSong:
                    if (CheckTimer())
                    {
                        ChooseNextSong();
                        SetPlayerState(MusicPlayerState.Playing);
                    }

                    break;
            }
        }

        #region Controls
        
        public void _Play()
        {
            if (Paused)
            {
                _Resume();
            }
            else
            {
                _Resume();
                if (SongIndex == -1) SongIndex = 0;
                SetPlayerState(MusicPlayerState.Playing);
            }
        }

        public void _Pause()
        {
            Paused = true;
            ButtonPlay.SetActive(true);
            ButtonPause.SetActive(false);
            Source.Pause();
        }

        public void _Resume()
        {
            Paused = false;
            ButtonPlay.SetActive(false);
            ButtonPause.SetActive(true);
            Source.UnPause();
        }

        public void _SkipForward()
        {
            if (PlaybackType == MusicPlaybackType.RepeatOne) SongIndex++;
            ChooseNextSong();
            SetPlayerState(MusicPlayerState.Playing);
        }

        public void _SkipBackwards()
        {
            ChooseNextSong(true);
            SetPlayerState(MusicPlayerState.Playing);
        }

        public void _Stop()
        {
            SetPlayerState(MusicPlayerState.Idle);
        }

        public void _Mute()
        {
            Muted = true;
            Source.volume = 0;
            ButtonVolumeUnmute.SetActive(true);
            ButtonVolumeMute.SetActive(false);
        }

        public void _Unmute()
        {
            Muted = false;
            Source.volume = SliderVolume.value;
            ButtonVolumeUnmute.SetActive(false);
            ButtonVolumeMute.SetActive(true);
        }

        public void _SetVolume()
        {
            if (Muted) _Unmute();
            Source.volume = SliderVolume.value;
            if (Source.volume < 0.33f)
            {
                VolumeMute.sprite = Volume0;
            }
            else if (Source.volume < 0.66f)
            {
                VolumeMute.sprite = Volume1;
            }
            else if (Source.volume >= 0.66f)
            {
                VolumeMute.sprite = Volume2;
            }
        }

        public void _SwitchPlaybackState()
        {
            MusicPlaybackType playbackType = MusicPlaybackType.Normal;
            switch (PlaybackType)
            {
                case MusicPlaybackType.Normal:
                    playbackType = MusicPlaybackType.RepeatAll;
                    break;
                case MusicPlaybackType.RepeatAll:
                    playbackType = MusicPlaybackType.RepeatOne;
                    break;
                case MusicPlaybackType.RepeatOne:
                    playbackType = MusicPlaybackType.Shuffle;
                    break;
                case MusicPlaybackType.Shuffle:
                    playbackType = MusicPlaybackType.Normal;
                    break;
            }

            SetPlayType(playbackType);
        }

        #endregion

        private void ChooseNextSong(bool reverse = false)
        {
            switch (PlaybackType)
            {
                case MusicPlaybackType.Normal:
                    if (reverse)
                    {
                        SongIndex--;
                        if (SongIndex < 0) SongIndex = songCount - 1;
                    }
                    else
                    {
                        SongIndex++;
                        if (SongIndex >= songCount) SongIndex = -1;
                    }
                    break;
                case MusicPlaybackType.RepeatAll:
                    if (reverse)
                    {
                        SongIndex--;
                        if (SongIndex < 0) SongIndex = songCount - 1;
                    }
                    else
                    {
                        SongIndex++;
                        if (SongIndex >= songCount) SongIndex = 0;
                    }
                    break;
                case MusicPlaybackType.RepeatOne:
                    if (reverse)
                    {
                        if (SongIndex < 0) SongIndex = songCount - 1;
                    }
                    else
                    {
                        if (SongIndex >= songCount) SongIndex = 0;
                    }
                    break;
                case MusicPlaybackType.Shuffle:
                    SongIndex = UnityEngine.Random.Range(0, songCount);
                    break;
            }
        }

        private void SetPlayType(MusicPlaybackType newPlayType)
        {
            PlaybackType = newPlayType;
            
            ButtonRepeat.SetActive(PlaybackType == MusicPlaybackType.Normal);
            ButtonRepeatAll.SetActive(PlaybackType == MusicPlaybackType.RepeatAll);
            ButtonRepeat1.SetActive(PlaybackType == MusicPlaybackType.RepeatOne);
            ButtonShuffle.SetActive(PlaybackType == MusicPlaybackType.Shuffle);
        }

        private void SetPlayerState(MusicPlayerState newState)
        {
            switch (newState)
            {
                case MusicPlayerState.Idle:
                    Source.Stop();
                    
                    ButtonPlay.SetActive(true);
                    ButtonPause.SetActive(false);
                    
                    SetProgressWheel(0);

                    ImageIcon.color = new Color(17, 17, 17);
                    ImageIcon.sprite = null;
                    TextSongName.text = string.Empty;
                    TextArtistName.text = string.Empty;
                    break;
                case MusicPlayerState.Playing:
                    if (SongIndex == -1)
                    {
                        _Stop();
                        return;
                    }
                    _Resume();
                    
                    AudioClip clip = SongClips[SongIndex];
                    LeaveStateTime = clip.length;
                    Source.clip = clip;
                    Source.Play();
                    
                    ImageIcon.color = Color.white;
                    ImageIcon.sprite = SongIcons[SongIndex];
                    TextSongName.text = SongNames[SongIndex];
                    TextArtistName.text = SongArtists[SongIndex];
                    
                    break;
                case MusicPlayerState.PostSong:
                    SetProgressWheel(1);
                    LeaveStateTime = PostSongWait;
                    break;
            }

            PlayerState = newState;
            StateTimer = 0;
        }
        
        private void UpdateVisuals()
        {
            SetProgressWheel(StateTimer / LeaveStateTime);
            Source.GetSpectrumData(audioSpectrum, 0, FFTWindow.Rectangular);
            for (int i = 0; i < PulseVisuals.Length; i++)
            {
                PulseVisuals[i].localScale = Vector3.Lerp(
                    PulseVisuals[i].localScale,
                    Vector3.Lerp(Vector3.one, Vector3.one * VisualizerMultiplier, audioSpectrum[i + 1] * (i + 3)),
                    VisualizerSmoothing);
            }
        }

        private void SetProgressWheel(float value)
        {
            WheelProgress.fillAmount = Mathf.Lerp(0, 1, value);
        }
        
        private bool CheckTimer()
        { 
            StateTimer += Time.deltaTime; 
            return StateTimer >= LeaveStateTime;
        }
        
    }
}