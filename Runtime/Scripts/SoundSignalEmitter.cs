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

        public SoundGroupAsset SoundGroup => soundGroup;
        public ActionType Action => action;

        public PropertyName id => new PropertyName(nameof(SoundSignalEmitter));
    }
}
