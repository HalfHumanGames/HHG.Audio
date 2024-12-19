using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HHG.Audio.Runtime
{
    public interface ITracksProvider
    {
        public void PopulateTracks(List<AssetReferenceT<AudioClip>> tracks, PlaylistAsset playlist);
    }
}