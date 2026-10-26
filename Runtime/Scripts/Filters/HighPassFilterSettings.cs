using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class HighPassFilterSettings
    {
        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField, Range(10f, 22000f)] private float cutoffFrequency = 5000f;
        [SerializeField, Range(1f, 10f)] private float highpassResonanceQ = 1f;

        private void Reset()
        {
            this.FromJsonOverwrite(new HighPassFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioHighPassFilter filter = source.GetOrAddComponent<AudioHighPassFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.cutoffFrequency = cutoffFrequency;
                filter.highpassResonanceQ = highpassResonanceQ;
            }
        }
    }
}
