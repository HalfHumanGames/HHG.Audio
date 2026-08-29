using UnityEngine;

namespace HHG.Audio.Runtime
{
    public class SoundEffect : MonoBehaviour
    {
        public SoundGroupAsset 
            Group => soundGroup;

        [SerializeField] private bool playOnAwake;
        [SerializeField] private SoundGroupAsset soundGroup;
        [SerializeField] private Sound.Space space;
        [SerializeField] private float delay;
        [SerializeField, ShowIf(nameof(space), Sound.Space._3D)] private Target target;
        [SerializeField, ShowIf(nameof(target), Target.Other)] private Transform targetTransform;

        private enum Target
        {
            Self,
            Other
        }

        private void Awake()
        {
            if (playOnAwake) Play();
        }

        public void Play()
        {
            switch (space)
            {
                case Sound.Space._2D:
                    Sound.PlayDelayed(soundGroup, delay);
                    break;

                default:
                    Vector3 position = target == Target.Self ? transform.position : targetTransform.position;
                    Sound.PlayDelayed(soundGroup, delay, position);
                    break;
            }
        }
    }
}