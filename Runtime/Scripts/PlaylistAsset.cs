using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace HHG.Audio.Runtime
{
    [CreateAssetMenu(fileName = "Playlist", menuName = "HHG/Audio/Playlist")]
    public class PlaylistAsset : ScriptableObject
    {
        // Load resizes tracks to _tracks.Count, so count non-null clips
        public bool IsLoaded => _tracks.Count > 0 && tracks.Count(t => t != null) == _tracks.Count;
        public bool PlayAll => playAll;
        public bool Shuffle => shuffle;
        public bool Loop => loop;
        public PlaylistAsset Chain => chain;
        public IReadOnlyList<AudioClip> Tracks => tracks;
        public IReadOnlyList<AssetReferenceT<AudioClip>> TrackReferences => _tracks;

        public event Action<PlaylistAsset> Loaded;

        [SerializeField] private bool playAll;
        [SerializeField] private bool shuffle;
        [SerializeField] private bool loop;
        [SerializeField] private PlaylistAsset chain;
        [SerializeField, FormerlySerializedAs("tracks")] private List<AssetReferenceT<AudioClip>> _tracks = new List<AssetReferenceT<AudioClip>>();
        [SerializeReference, SubclassSelector] private ITracksProvider tracksProvider;

        private List<AudioClip> tracks = new List<AudioClip>();
        private List<AsyncOperationHandle<AudioClip>> handles = new List<AsyncOperationHandle<AudioClip>>();
        private List<AsyncOperationHandle<AudioClip>> previousHandles = new List<AsyncOperationHandle<AudioClip>>();

        private void OnEnable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Reset();
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            Reset();
        }

        private void Reset()
        {
            // Only clear tracks if using a tracks provider
            if (tracksProvider != null)
            {
                // Playlists keep playing the previous tracks across scenes, so release them only once the next tracks load
                if (handles.Count > 0)
                {
                    ReleaseHandles(previousHandles);
                    previousHandles.AddRange(handles);
                    handles.Clear();
                }

                tracks.Clear();
            }
        }

        // Unloads the tracks once nothing plays this playlist anymore
        public void Release()
        {
            ReleaseHandles(handles);
            ReleaseHandles(previousHandles);
            tracks.Clear();
        }

        private static void ReleaseHandles(List<AsyncOperationHandle<AudioClip>> handlesToRelease)
        {
            foreach (AsyncOperationHandle<AudioClip> handle in handlesToRelease)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            handlesToRelease.Clear();
        }

        public void Load()
        {
            // Handles exist once a load starts, so loading twice doesn't duplicate them
            if (handles.Count == 0)
            {
                if (tracksProvider != null)
                {
                    tracksProvider.PopulateTracks(_tracks, this);
                }

                tracks.Resize(_tracks.Count);

                for (int i = 0; i < _tracks.Count; i++)
                {
                    int index = i; // Can't use i since it changes
                    AssetReferenceT<AudioClip> reference = _tracks[i];
                    AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(reference);
                    handles.Add(handle);

                    handle.Completed += (operation) =>
                    {
                        // Ignore loads that were reset or released before they finished
                        if (!handles.Contains(operation))
                        {
                            return;
                        }

                        if (operation.Status == AsyncOperationStatus.Succeeded)
                        {
                            AudioClip track = operation.Result;

                            // Add by index to make sure 
                            // tracks get added in order
                            tracks[index] = track;

                            if (IsLoaded)
                            {
                                ReleaseHandles(previousHandles);
                                Loaded?.Invoke(this);
                            }
                        }
                        else if (operation.Status == AsyncOperationStatus.Failed)
                        {
                            Debug.LogError($"{operation.OperationException.Message}\n{operation.OperationException.StackTrace}", this);
                        }
                    };
                }
            }
        }
    }
}