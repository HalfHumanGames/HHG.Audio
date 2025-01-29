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
        private bool isLoading;

        private void Start()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.priority = 0;
            source.spatialBlend = 0f;
            PlayPlaylist();
        }

        private void Update()
        {
            if (!isLoading && !source.isPlaying)
            {
                PlayNextTrack();
            }
        }

        private void PlayPlaylist(PlaylistAsset nextPlaylist)
        {
            playlist = nextPlaylist;
            PlayPlaylist();
        }

        private void PlayPlaylist()
        {
            if (playlist == null)
            {
                return;
            }

            if (!playlist.IsLoaded)
            {
                isLoading = true;
                playlist.Loaded += OnPlaylistLoaded;
                playlist.Load();
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

        private void OnPlaylistLoaded(PlaylistAsset playlist)
        {
            isLoading = false;
            playlist.Loaded -= OnPlaylistLoaded;
            PlayPlaylist();
        }

        [ContextMenu("Play Next Track")]
        private void PlayNextTrack()
        {
            if (playlist == null)
            {
                Debug.LogError("'playlist' cannot be null.", this);
                return;
            }

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