using HHG.Common.Runtime;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    public class DistortionFilterSettings
    {
        [EditorButton(nameof(Reset), "Reset", PositionType = ButtonPositionType.Above)]
        [SerializeField, Range(0f, 1f)] private float distortionLevel = .5f;

        private void Reset()
        {
            this.FromJsonOverwrite(new DistortionFilterSettings());
        }

        public void Apply(AudioSource source, bool enabled)
        {
            AudioDistortionFilter filter = source.GetOrAddComponent<AudioDistortionFilter>();
            filter.enabled = enabled;

            if (enabled)
            {
                filter.distortionLevel = distortionLevel;
            }
        }
    }
}
