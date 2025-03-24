using UnityEngine;

namespace HHG.Audio.Runtime
{
    public class SoundLoopHandle
    {
        public AudioSource Source => source;
        public Coroutine Coroutine => coroutine;

        private AudioSource source;
        private Coroutine coroutine;

        public SoundLoopHandle(AudioSource audioSource)
        {
            source = audioSource;
        }

        public SoundLoopHandle(AudioSource audioSource, Coroutine tweenCoroutine) : this(audioSource)
        {
            coroutine = tweenCoroutine;
        }
    }
}