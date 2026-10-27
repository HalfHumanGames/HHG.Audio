using HHG.Common.Runtime;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace HHG.Audio.Runtime
{
    /* Additonal features:
        - https://www.dtdevtools.com/docs/masteraudio/Occlusion.htm
        - https://www.dtdevtools.com/docs/masteraudio/AudioDucking.htm
        - https://www.dtdevtools.com/docs/masteraudio/FilterFX.htm
    */
    [Serializable]
    public partial class Sound
    {
        public const float SilentDb = -80f;

        public bool IsLoaded => clip != null;
        public AudioClip Clip => clip;
        public float VolumeOffsetDb => volumeOffsetDb.NextRand;
        public float PitchOffsetSemitones => pitchOffsetSemitones.NextRand;
        public float DelayOffsetSeconds => delayOffsetSeconds.NextRand;
        public int Weight => weight;

        public event Action<Sound> Loaded;

        [SerializeField] private AssetReferenceT<AudioClip> _clip;
        [SerializeField, Row] private MinMaxFloat volumeOffsetDb = 0f;
        [SerializeField, Row] private MinMaxFloat pitchOffsetSemitones = 0f;
        [SerializeField, Row] private MinMaxFloat delayOffsetSeconds = 0f;
        [SerializeField, Min(1)] private int weight = 1;

        // Unity caches this across play and edit mode
        // unless we add this NonSerialized attribute
        // Without it, sounds plays stale audio slips
        [NonSerialized] private AudioClip clip;

        // dB to linear gain (0 dB = 1). SilentDb or lower is treated as silent.
        public static float DecibelsToLinear(float db)
        {
            return db <= SilentDb ? 0f : Mathf.Pow(10f, db / 20f);
        }

        // Semitones to playback rate (0 st = 1, 12 st = 2).
        public static float SemitonesToPitch(float semitones)
        {
            return Mathf.Pow(2f, semitones / 12f);
        }

        public Sound(string guid)
        {
            _clip = new AssetReferenceT<AudioClip>(guid);
        }

        public void Load(bool force = false)
        {
            if (force || clip == null)
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
                        Debug.LogError($"{handle.OperationException.Message}\n{handle.OperationException.StackTrace}");
                    }
                };
            }
        }
    }
}