using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace HHG.Audio.Runtime
{
    [CustomStyle("SignalEmitter")]
    public class SoundSignalEmitter : Marker, INotification
    {
        public enum ActionType
        {
            Play,
            Stop
        }

        [SerializeField] private SoundGroupAsset soundGroup;
        [SerializeField] private ActionType action;
        [SerializeField, ShowIf(nameof(isPlay), true)] private bool loop;

        public SoundGroupAsset SoundGroup => soundGroup;
        public ActionType Action => action;
        public bool Loop => loop;

        private bool isPlay => action == ActionType.Play;

        public PropertyName id => new PropertyName(nameof(SoundSignalEmitter));
    }
}
