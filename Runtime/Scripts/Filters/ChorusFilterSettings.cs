using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class ChorusFilterSettings
    {
        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField, Range(0f, 1f)] private float dryMix = .5f;
        [SerializeField, Range(0f, 1f)] private float wetMix1 = .5f;
        [SerializeField, Range(0f, 1f)] private float wetMix2 = .5f;
        [SerializeField, Range(0f, 1f)] private float wetMix3 = .5f;
        [SerializeField, Range(.1f, 100f)] private float delay = 40f;
        [SerializeField, Range(0f, 20f)] private float rate = .8f;
        [SerializeField, Range(0f, 1f)] private float depth = .03f;

        private void Reset()
        {
            this.FromJsonOverwrite(new ChorusFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioChorusFilter filter = source.GetOrAddComponent<AudioChorusFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.dryMix = dryMix;
                filter.wetMix1 = wetMix1;
                filter.wetMix2 = wetMix2;
                filter.wetMix3 = wetMix3;
                filter.delay = delay;
                filter.rate = rate;
                filter.depth = depth;
            }
        }
    }
}
