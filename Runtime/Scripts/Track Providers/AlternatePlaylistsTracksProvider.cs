using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HHG.Audio.Runtime
{
    [Serializable]
    public class AlternatePlaylistsTracksProvider : ITracksProvider
    {
        [SerializeField] private List<PlaylistAsset> playlists = new List<PlaylistAsset>();

        public void PopulateTracks(List<AssetReferenceT<AudioClip>> tracks, PlaylistAsset playlist)
        {
            if (playlists.Count == 0)
            {
                Debug.LogError("You must have at least 1 playlist!", playlist);
                return;
            }

            int trackCount = playlists[0].TrackReferences.Count;

            if (trackCount == 0)
            {
                Debug.LogError("Playlists must contain at least 1 track!", playlist);
                return;
            }

            if (playlists.Any(p => p.TrackReferences.Count != trackCount))
            {
                Debug.LogError("All playlists must have the same number of tracks!", playlist);
                return;
            }

            tracks.Clear();

            List<List<AssetReferenceT<AudioClip>>> trackReferences = new List<List<AssetReferenceT<AudioClip>>>();

            for (int i = 0; i < playlists.Count; i++)
            {
                trackReferences.Add(new List<AssetReferenceT<AudioClip>>(playlists[i].TrackReferences));

                if (playlists[i].Shuffle)
                {
                    trackReferences[i].Shuffle();
                }
            }

            for (int i = 0; i < trackCount; i++)
            {
                for (int j = 0; j < trackReferences.Count; j++)
                {
                    tracks.Add(trackReferences[j][i]);
                }
            }
        }
    }
}