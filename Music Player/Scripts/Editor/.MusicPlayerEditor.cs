
using System.Collections.Generic;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace Vowgan.Music
{
    [CustomEditor(typeof(MusicPlayer))]
    public class MusicPlayerEditor : Editor
    {
        
        private MusicPlayer script;

        private bool useLargePreview;
        private bool showDefaultInspector; 
        
        private void OnEnable()
        {
            script = target as MusicPlayer;
            if (script == null) return;

            if (EditorPrefs.HasKey(prefKeyUseLargePreview))
            {
                useLargePreview = EditorPrefs.GetBool(prefKeyUseLargePreview);
            }
            if (EditorPrefs.HasKey(prefKeyShowDefaultInspector))
            {
                showDefaultInspector = EditorPrefs.GetBool(prefKeyShowDefaultInspector);
            }
            
            if (script.SongIcons == null)
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
            
            script.SongIcons = tempIcons;
            script.SongClips = tempClips;
            script.SongNames = tempTitles;
            script.SongArtists = tempArtists;
            
            script.ApplyProxyModifications();
        }
        
        private void PullFromScript()
        {
            int length = script.SongIcons.Length;
            songs = new List<VVRMusicSong>(0);
            for (int i = 0; i < length; i++)
            {
                songs.Add(new VVRMusicSong());
                songs[i].icon = script.SongIcons[i];
                songs[i].clip = script.SongClips[i];
                songs[i].title = script.SongNames[i];
                songs[i].artist = script.SongArtists[i];
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
}

