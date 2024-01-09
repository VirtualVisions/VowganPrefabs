using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Vowgan.Music
{
    public class MusicPlayerScenePostProcess : MonoBehaviour
    {

        [PostProcessScene(-10)]
        public static void OnPostProcessScene()
        {
            List<MusicPlayer> players =
                FindObjectsByType<MusicPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();

            foreach (MusicPlayer player in players)
            {
                if (!player.Playlist) continue;

                List<string> songNames = new List<string>();
                List<string> songArtists = new List<string>();
                List<AudioClip> songClips = new List<AudioClip>();
                List<Sprite> songIcons = new List<Sprite>();

                foreach (SongPreset song in ((MusicPlayerPlaylist)player.Playlist).Songs)
                {
                    songNames.Add(song.Name);
                    songArtists.Add(song.Artist);
                    songClips.Add(song.Clip);
                    songIcons.Add(song.Icon);
                }

                player.SongNames = songNames.ToArray();
                player.SongArtists = songArtists.ToArray();
                player.SongClips = songClips.ToArray();
                player.SongIcons = songIcons.ToArray();
            }
        }
        
    }
}