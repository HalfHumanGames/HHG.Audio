using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class EchoFilterSettings
    {
        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField, Range(10f, 5000f)] private float delay = 500f;
        [SerializeField, Range(0f, 1f)] private float decayRatio = .5f;
        [SerializeField, Range(0f, 1f)] private float dryMix = 1f;
        [SerializeField, Range(0f, 1f)] private float wetMix = 1f;

        private void Reset()
        {
            this.FromJsonOverwrite(new EchoFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioEchoFilter filter = source.GetOrAddComponent<AudioEchoFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.delay = delay;
                filter.decayRatio = decayRatio;
                filter.dryMix = dryMix;
                filter.wetMix = wetMix;
            }
        }
    }
}
