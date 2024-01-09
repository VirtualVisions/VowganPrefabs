using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vowgan.Music
{
    [CreateAssetMenu(menuName = "Vowgan/Music Playlist", fileName = "Playlist")]
    public class MusicPlayerPlaylist : ScriptableObject
    {
        public List<SongPreset> Songs = new List<SongPreset>();
    }
    
    [Serializable]
    public class SongPreset
    {
        public string Name;
        public string Artist;
        public AudioClip Clip;
        public Sprite Icon;
    }
}