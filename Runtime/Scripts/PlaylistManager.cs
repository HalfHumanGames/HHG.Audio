using HHG.Common.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class PlaylistManager : MonoBehaviour
    {
        [SerializeField] private PlaylistAsset playlist;

        private List<AudioClip> tracks = new List<AudioClip>();
        private AudioSource source;
        private int current = 0;

        private void Start()
        {
            source = GetComponent<AudioSource>();
            PlayPlaylist();
        }

        private void Update()
        {
            if (!source.isPlaying)
            {
                PlayNextTrack();
            }
        }

        private void PlayPlaylist(PlaylistAsset playlist)
        {
            this.playlist = playlist;
            PlayPlaylist();
        }

        private void PlayPlaylist()
        {
            if (playlist == null)
            {
                return;
            }

            if (playlist.Tracks.Count == 0)
            {
                return;
            }

            tracks.Clear();
            tracks.AddRange(playlist.Tracks);

            if (playlist.Shuffle)
            {
                tracks.Shuffle();
            }

            current = 0;

            PlayNextTrack();
        }

        [ContextMenu("Play Next Track")]
        private void PlayNextTrack()
        {
            if (current < tracks.Count)
            {
                source.clip = tracks[current++];
                source.Play();

                if (!playlist.PlayAll)
                {
                    current = tracks.Count;
                }
            }
            else if (playlist.Loop)
            {
                PlayPlaylist();
            }
            else if (playlist.Chain)
            {
                PlayPlaylist(playlist.Chain);
            }
        }
    }
}