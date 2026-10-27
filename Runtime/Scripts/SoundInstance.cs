using System.Collections.Generic;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    internal class SoundInstance
    {
        public SoundGroupAsset Group;
        public AudioSource Source;
        public Coroutine Fade;
        public System.Action<SoundGroupAsset> LoadedHandler;
        public bool IsLooping;
        public bool IsStopping;
        public int Version;
        public readonly List<SoundHandle> Layers = new List<SoundHandle>();

        public bool HasActiveLayers
        {
            get
            {
                foreach (SoundHandle layer in Layers)
                {
                    if (layer.IsValid)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Bumping the version invalidates every handle to this instance before it gets reused
        public void Reset()
        {
            Group = null;
            Source = null;
            Fade = null;
            LoadedHandler = null;
            IsLooping = false;
            IsStopping = false;
            Layers.Clear();
            Version++;
        }
    }
}
