using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace HHG.Audio.Runtime
{
    [CreateAssetMenu(fileName = "Playlist", menuName = "HHG/Audio/Playlist")]
    public class PlaylistAsset : ScriptableObject
    {
        // Load resizes tracks to _tracks.Count, so count non-null clips
        public bool IsLoaded => tracks.Count(t => t != null) == _tracks.Count;
        public bool PlayAll => playAll;
        public bool Shuffle => shuffle;
        public bool Loop => loop;
        public PlaylistAsset Chain => chain;
        public IReadOnlyList<AudioClip> Tracks => tracks;

        public event Action<PlaylistAsset> Loaded;

        [SerializeField] private bool playAll;
        [SerializeField] private bool shuffle;
        [SerializeField] private bool loop;
        [SerializeField] private PlaylistAsset chain;
        [SerializeField, FormerlySerializedAs("tracks")] private List<AssetReferenceT<AudioClip>> _tracks = new List<AssetReferenceT<AudioClip>>();

        private List<AudioClip> tracks = new List<AudioClip>();

        public void Load()
        {
            if (!IsLoaded)
            {
                tracks.Resize(_tracks.Count);

                for (int i = 0; i < _tracks.Count; i++)
                {
                    int index = i; // Can't use i since it changes
                    AssetReferenceT<AudioClip> reference = _tracks[i];
                    AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(reference);

                    handle.Completed += (operation) =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            AudioClip track = handle.Result;

                            // Add by index to make sure 
                            // tracks get added in order
                            tracks[index] = track;

                            if (IsLoaded)
                            {
                                Loaded?.Invoke(this);
                            }
                        }
                        else if (handle.Status == AsyncOperationStatus.Failed)
                        {
                            DebugUtil.LogException(handle.OperationException.Message + "\n" + handle.OperationException.StackTrace);
                        }
                    };
                }
            }
        }
    }
}