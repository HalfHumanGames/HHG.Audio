using HHG.Common.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class PlaylistManager : MonoBehaviour
    {
        [System.Serializable]
        private struct Priority
        {
            public int Start;
            public int End;
        }

        private static PlaylistManager current;

        [SerializeField] private float fadeDuration = 1f;
        [SerializeField, Row] private Priority priority;
        [SerializeField] private PlaylistAsset playlist;

        private List<AudioClip> tracks = new List<AudioClip>();
        private AudioSource source;
        private int currentTrackIndex = 0;
        private bool isLoading;
        private bool shouldFadeIn;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.priority = 0;
            source.spatialBlend = 0f;

            if (current)
            {
                if (current.priority.End < priority.Start && current.playlist != playlist) 
                {
                    shouldFadeIn = true;
                    current.FadeOutThenDestroySelf();
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            current = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
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
                Debug.LogError("playlist is null.", this);
                return;
            }

            if (!playlist.IsLoaded)
            {
                isLoading = true;
                playlist.Loaded += OnPlaylistLoaded;
                playlist.Load();
                return;
            }

            if (playlist.Tracks == null)
            {
                Debug.LogError("playlist.Tracks is null.", this);
                return;
            }

            if (playlist.Tracks.Count == 0)
            {
                return;
            }

            if (tracks == null)
            {
                Debug.LogError("tracks is null.", this);
                return;
            }

            tracks.Clear();
            tracks.AddRange(playlist.Tracks);

            if (playlist.Shuffle)
            {
                tracks.Shuffle();
            }

            currentTrackIndex = 0;

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
                Debug.LogError("playlist is null.", this);
                return;
            }

            if (tracks == null)
            {
                Debug.LogError("tracks is null.", this);
                return;
            }

            if (currentTrackIndex < tracks.Count)
            {
                if (source == null)
                {
                    Debug.LogError("source is null.", this);
                    return;
                }

                source.clip = tracks[currentTrackIndex++];

                if (shouldFadeIn)
                {
                    float volume = source.volume;
                    source.volume = 0f;
                    source.FadeTo(volume, fadeDuration);
                }
                else
                {
                    source.Play();
                }
               
                if (!playlist.PlayAll)
                {
                    currentTrackIndex = tracks.Count;
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

        private void FadeOutThenDestroySelf()
        {
            StartCoroutine(FadeOutThenDestroySelfAsync());
        }

        private IEnumerator FadeOutThenDestroySelfAsync()
        {
            yield return source.FadeTo(0f, fadeDuration);
            Destroy(gameObject);
        }
    }
}