using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class LowPassFilterSettings
    {
        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField, Range(10f, 22000f)] private float cutoffFrequency = 5000f;
        [SerializeField, Range(1f, 10f)] private float lowpassResonanceQ = 1f;

        private void Reset()
        {
            this.FromJsonOverwrite(new LowPassFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioLowPassFilter filter = source.GetOrAddComponent<AudioLowPassFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.cutoffFrequency = cutoffFrequency;
                filter.lowpassResonanceQ = lowpassResonanceQ;
            }
        }
    }
}
