using UnityEngine;

namespace HHG.Audio.Common
{
    public class SfxLoopHandle
    {
        public AudioSource Source => source;
        public Coroutine Coroutine => coroutine;

        private AudioSource source;
        private Coroutine coroutine;

        public SfxLoopHandle(AudioSource audioSource)
        {
            source = audioSource;
        }

        public SfxLoopHandle(AudioSource audioSource, Coroutine tweenCoroutine) : this(audioSource)
        {
            coroutine = tweenCoroutine;
        }
    }
}