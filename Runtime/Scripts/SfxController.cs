using UnityEngine;

namespace HHG.Audio.Runtime
{
    public class SfxController : MonoBehaviour
    {
        public SfxGroupAsset SfxGroup => sfxGroup;

        [SerializeField] private Sfx.Space space;
        [SerializeField] private SfxGroupAsset sfxGroup;

        public void Play()
        {
            switch (space)
            {
                case Sfx.Space._2D:
                    Sfx.Play(sfxGroup);
                    break;

                default:
                    Sfx.Play(sfxGroup, transform.position);
                    break;
            }
        }
    }
}