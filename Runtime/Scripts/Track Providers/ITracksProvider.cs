using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    public interface ITracksProvider
    {
        public void PopulateTracks(List<AssetReferenceT<AudioClip>> tracks, PlaylistAsset playlist);
    }
}