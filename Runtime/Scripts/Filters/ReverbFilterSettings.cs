using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class ReverbFilterSettings
    {
        private bool ShowCustom => reverbPreset == AudioReverbPreset.User;

        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField] private AudioReverbPreset reverbPreset = AudioReverbPreset.Generic;
        [SerializeField, Range(-10000f, 0f), ShowIf(nameof(ShowCustom), true)] private float dryLevel;
        [SerializeField, Range(-10000f, 0f), ShowIf(nameof(ShowCustom), true)] private float room;
        [SerializeField, Range(-10000f, 0f), ShowIf(nameof(ShowCustom), true)] private float roomHF;
        [SerializeField, Range(-10000f, 0f), ShowIf(nameof(ShowCustom), true)] private float roomLF;
        [SerializeField, Range(.1f, 20f), ShowIf(nameof(ShowCustom), true)] private float decayTime = 1f;
        [SerializeField, Range(.1f, 2f), ShowIf(nameof(ShowCustom), true)] private float decayHFRatio = .5f;
        [SerializeField, Range(-10000f, 1000f), ShowIf(nameof(ShowCustom), true)] private float reflectionsLevel = -10000f;
        [SerializeField, Range(0f, .3f), ShowIf(nameof(ShowCustom), true)] private float reflectionsDelay;
        [SerializeField, Range(-10000f, 2000f), ShowIf(nameof(ShowCustom), true)] private float reverbLevel;
        [SerializeField, Range(0f, .1f), ShowIf(nameof(ShowCustom), true)] private float reverbDelay = .04f;
        [SerializeField, Range(1000f, 20000f), ShowIf(nameof(ShowCustom), true)] private float hfReference = 5000f;
        [SerializeField, Range(20f, 1000f), ShowIf(nameof(ShowCustom), true)] private float lfReference = 250f;
        [SerializeField, Range(0f, 100f), ShowIf(nameof(ShowCustom), true)] private float diffusion = 100f;
        [SerializeField, Range(0f, 100f), ShowIf(nameof(ShowCustom), true)] private float density = 100f;

        private void Reset()
        {
            this.FromJsonOverwrite(new ReverbFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioReverbFilter filter = source.GetOrAddComponent<AudioReverbFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.reverbPreset = reverbPreset;

                if (reverbPreset == AudioReverbPreset.User)
                {
                    filter.dryLevel = dryLevel;
                    filter.room = room;
                    filter.roomHF = roomHF;
                    filter.roomLF = roomLF;
                    filter.decayTime = decayTime;
                    filter.decayHFRatio = decayHFRatio;
                    filter.reflectionsLevel = reflectionsLevel;
                    filter.reflectionsDelay = reflectionsDelay;
                    filter.reverbLevel = reverbLevel;
                    filter.reverbDelay = reverbDelay;
                    filter.hfReference = hfReference;
                    filter.lfReference = lfReference;
                    filter.diffusion = diffusion;
                    filter.density = density;
                }
            }
        }
    }
}
