using HHG.Common.Runtime;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HHG.Audio.Runtime
{
    /* Additonal features:
        - https://www.dtdevtools.com/docs/masteraudio/Occlusion.htm
        - https://www.dtdevtools.com/docs/masteraudio/AudioDucking.htm
        - https://www.dtdevtools.com/docs/masteraudio/FilterFX.htm
    */
    [Serializable]
    public partial class Sfx
    {
        public bool IsLoaded => clip != null;
        public AudioClip Clip => clip;
        //public bool Loop => loop;
        public float Volume => volumeRange.NextRand;
        public float Pitch => pitchRange.NextRand;
        public float Delay => delayRange.NextRand;
        public int Weight => weight;

        public event Action<Sfx> Loaded;

        [SerializeField] private AssetReferenceT<AudioClip> _clip;
        //[SerializeField] private bool loop;
        [SerializeField, Row] private MinMaxFloat volumeRange = 1;
        [SerializeField, Row] private MinMaxFloat pitchRange = 1;
        [SerializeField, Row] private MinMaxFloat delayRange = 0;
        [SerializeField] private int weight = 1;

        private AudioClip clip;

        public void Load()
        {
            if (clip == null)
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(_clip);

                handle.Completed += (operation) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        clip = handle.Result;

                        Loaded?.Invoke(this);
                    }
                    else if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError(handle.OperationException.Message + "\n" + handle.OperationException.StackTrace);
                    }
                };
            }
        }
    }
}