using UnityEngine;

namespace HHG.Audio.Runtime
{
    public class SoundEffect : MonoBehaviour
    {
        public SoundGroupAsset 
            Group => soundGroup;

        [SerializeField] private Sound.Space space;
        [SerializeField] private SoundGroupAsset soundGroup;

        public void Play()
        {
            switch (space)
            {
                case Sound.Space._2D:
                    Sound.Play(soundGroup);
                    break;

                default:
                    Sound.Play(soundGroup, transform.position);
                    break;
            }
        }
    }
}