using UnityEngine;
using UnityEngine.Playables;

namespace HHG.Audio.Runtime
{
    public class SoundSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] private bool playAtTransform;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SoundSignalEmitter signalEmitter)
            {
                switch (signalEmitter.Action)
                {
                    case SoundSignalEmitter.ActionType.Play:
                        if (playAtTransform)
                        {
                            Sound.Play(signalEmitter.SoundGroup, transform.position);
                        }
                        else
                        {
                            Sound.Play(signalEmitter.SoundGroup);
                        }
                        break;

                    case SoundSignalEmitter.ActionType.Stop:
                        Sound.Stop(signalEmitter.SoundGroup);
                        break;
                }
            }
        }
    }
}
