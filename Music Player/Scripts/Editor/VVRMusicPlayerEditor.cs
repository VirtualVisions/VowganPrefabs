using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace VowganVR
{
    [CustomEditor(typeof(VVRMusicPlayer))]
    public class VVRMusicPlayerEditor : Editor
    {
        
        private VVRMusicPlayer script;
        private List<VVRMusicSong> songs = new List<VVRMusicSong>(0);
        private const string prefKeyUseLargePreview = "Vowgan/MusicPlayerUseLargePreview";
        private const string prefKeyShowDefaultInspector = "Vowgan/MusicPlayerShowDefaultInspector";

        private bool useLargePreview;
        private bool showDefaultInspector;
        
        private void OnEnable()
        {
            script = target as VVRMusicPlayer;
            if (script == null) return;

            if (EditorPrefs.HasKey(prefKeyUseLargePreview))
            {
                useLargePreview = EditorPrefs.GetBool(prefKeyUseLargePreview);
            }
            if (EditorPrefs.HasKey(prefKeyShowDefaultInspector))
            {
                showDefaultInspector = EditorPrefs.GetBool(prefKeyShowDefaultInspector);
            }
            
            if (script.songIcons == null)
            {
                ConvertToArrays();
            }
            else
            {
                PullFromScript();
            }
            
            if (songs.Count == 0)
            {
                songs.Add(new VVRMusicSong());
            }
            
            Undo.undoRedoPerformed += PullFromScript;
        }
        
        public override void OnInspectorGUI()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("VVR Music Player | Vowgan", GUILayout.Height(30));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            PlaybackTypeDropdown();
            TogglePlayOnStart();
            ToggleLargePreview();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DisplayAddRemoveButtons();
            GUILayout.EndHorizontal();
            
            SongList();
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DisplayAddRemoveButtons();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            if (ReferenceToggle())
            {
                if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
                DrawDefaultInspector();
            }
        }
        
        private void SongList()
        {
            for (int i = 0; i < songs.Count; i++)
            {
                var song = songs[i];

                GUILayout.BeginHorizontal(GUI.skin.box);
                
                song.Preview(useLargePreview);
                
                if (useLargePreview)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(20));
                    if (GUILayout.Button("▴", EditorStyles.miniLabel,GUILayout.Height(20)))
                    {
                        ReorderList(i, -1);
                    }
                    if (GUILayout.Button("✕", EditorStyles.miniLabel,GUILayout.Height(20)))
                    {
                        Undo.RecordObject(script, "Reordered List");
                        if (songs.Count == 1)
                        {
                            songs[i] = new VVRMusicSong();
                        }
                        else
                        {
                            songs.RemoveAt(i);
                        }
                        ConvertToArrays();
                    }
                    if (GUILayout.Button("▾", EditorStyles.miniLabel,GUILayout.Height(20)))
                    {
                        ReorderList(i, 1);
                    }
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Width(20));
                    if (GUILayout.Button("▴", EditorStyles.miniLabel,GUILayout.Height(18)))
                    {
                        ReorderList(i, -1);
                    }
                    if (GUILayout.Button("✕", EditorStyles.miniLabel,GUILayout.Height(18)))
                    {
                        Undo.RecordObject(script, "Reordered List");
                        songs.RemoveAt(i);
                        ConvertToArrays();
                    }
                    if (GUILayout.Button("▾", EditorStyles.miniLabel,GUILayout.Height(18)))
                    {
                        ReorderList(i, 1);
                    }
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndHorizontal();
                
                if (song.changed)
                {
                    song.changed = false;
                    ConvertToArrays();
                }
            }
        }
        
        private void DisplayAddRemoveButtons()
        {
            EditorGUI.BeginChangeCheck();
            
            if (GUILayout.Button("+", EditorStyles.miniButtonLeft, GUILayout.Width(20)))
            {
                Undo.RecordObject(script, "Added new song index");
                songs.Add(new VVRMusicSong());
                ConvertToArrays();
            }
            else if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
            {
                Undo.RecordObject( script, "Removed last song index");
                songs.RemoveAt(songs.Count - 1);
                if (songs.Count == 0)
                {
                    songs.Add(new VVRMusicSong());
                }
                ConvertToArrays();
            }
            
        }

        private bool ReferenceToggle()
        {
            EditorGUI.BeginChangeCheck();
            
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Show Default Inspector", true);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(prefKeyShowDefaultInspector, showDefaultInspector);
            }
            
            return showDefaultInspector;
        }

        private void ToggleLargePreview()
        {
            EditorGUI.BeginChangeCheck();

            useLargePreview = EditorGUILayout.Toggle("Large Mode", useLargePreview);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(prefKeyUseLargePreview, useLargePreview);
            }
        }
        
        private void PlaybackTypeDropdown()
        {
            EditorGUI.BeginChangeCheck();
            
            var playbackType = (PlaybackType) EditorGUILayout.EnumPopup("Playback Type", (PlaybackType)script.playbackType);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, $"Changed PlaybackType");
                script.playbackType = (int) playbackType;
                script.ApplyProxyModifications();
            }
        }

        private void TogglePlayOnStart()
        {
            EditorGUI.BeginChangeCheck();

            var playOnStart = EditorGUILayout.Toggle("Play on Start", script.playOnStart);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, "Toggled PlayOnStart");
                script.playOnStart = playOnStart;
                script.ApplyProxyModifications();
            }
        }
        
        private void ConvertToArrays()
        {
            Undo.RecordObject(script, "Pushed new values to Behaviour");
            
            int length = songs.Count;
            Sprite[] tempIcons = new Sprite[length];
            AudioClip[] tempClips = new AudioClip[length];
            string[] tempTitles = new string[length];
            string[] tempArtists = new string[length];
            
            for (int i = 0; i < length; i++)
            {
                tempIcons[i] = songs[i].icon;
                tempClips[i] = songs[i].clip;
                tempTitles[i] = songs[i].title;
                tempArtists[i] = songs[i].artist;
            }
            
            script.songIcons = tempIcons;
            script.songClips = tempClips;
            script.songTitles = tempTitles;
            script.songArtists = tempArtists;
            
            script.ApplyProxyModifications();
        }
        
        private void PullFromScript()
        {
            int length = script.songIcons.Length;
            songs = new List<VVRMusicSong>(0);
            for (int i = 0; i < length; i++)
            {
                songs.Add(new VVRMusicSong());
                songs[i].icon = script.songIcons[i];
                songs[i].clip = script.songClips[i];
                songs[i].title = script.songTitles[i];
                songs[i].artist = script.songArtists[i];
            }
        }
        
        private void ReorderList(int index, int change)
        {
            var changeDelta = index + change;
            if (changeDelta < 0 || changeDelta >= songs.Count) return;
            Undo.RecordObject(script, "Reordered List");
            
            var item = songs[index];
            songs.RemoveAt(index);
            songs.Insert(changeDelta, item);
            ConvertToArrays();
        }
    }
    
    
    public class VVRMusicSong
    {
        public Sprite icon;
        public AudioClip clip;
        public string title;
        public string artist;
        public bool changed;
        
        public void Preview(bool useLargeSize)
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            if (useLargeSize)
            {
                icon = (Sprite) EditorGUILayout.ObjectField(icon, typeof(Sprite), false, GUILayout.Height(64), GUILayout.Width(64));
                
                GUILayout.BeginVertical();
                
                clip = (AudioClip) EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Title", GUILayout.Width(64));
                title = EditorGUILayout.TextField(title);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Artist", GUILayout.Width(64));
                artist = EditorGUILayout.TextField(artist);
                GUILayout.EndHorizontal();
                
                GUILayout.EndVertical();
            }
            else
            {
                icon = (Sprite) EditorGUILayout.ObjectField(icon, typeof(Sprite), false);

                clip = (AudioClip) EditorGUILayout.ObjectField(clip, typeof(AudioClip), false);
                title = EditorGUILayout.TextField(title);
                artist = EditorGUILayout.TextField(artist);
            }
            
            GUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }
        }
    }
    
    public enum PlaybackType : byte
    {
        Normal,
        RepeatAll,
        RepeatOne,
        Shuffle,
    }
    
}

