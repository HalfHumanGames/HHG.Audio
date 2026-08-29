using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace HHG.Audio.Runtime
{
    [CreateAssetMenu(fileName = "Sound Group", menuName = "HHG/Audio/Sound Group")]
    public class SoundGroupAsset : StringNameAsset
    {
        public bool IsLoaded => LoadedCount == sounds.Count;
        public int LoadedCount => sounds.Count(sound => sound.IsLoaded);
        public List<Sound> Sounds => sounds;

        public event Action<SoundGroupAsset> Loaded;

        [SerializeField] private float playChance = 1f;
        [SerializeField] private int priority;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private float volume = 1f;
        [SerializeField] private float pitch = 1f;
        [SerializeField] private float cooldown = .2f;
        [SerializeField] private int maxVoices = 5;
        [SerializeField, Row] private MinMaxFloat distance = new MinMaxFloat(1f, 500f);
        [SerializeField] private AudioRolloffMode rolloffMode;
        [SerializeField] private AnimationCurve customRolloff;
        [SerializeField] private List<Sound> sounds = new List<Sound>();

        public void Load()
        {
            if (!IsLoaded)
            {
                foreach (Sound sound in sounds)
                {
                    sound.Loaded += OnLoaded;
                    sound.Load();
                }
            }
        }

        private void OnLoaded(Sound sound)
        {
            sound.Loaded -= OnLoaded;

            if (IsLoaded)
            {
                Loaded?.Invoke(this);
            }
        }

        public bool CanPlay(int voiceCount, float timestamp)
        {
            return voiceCount < maxVoices && Time.unscaledTime - timestamp > cooldown && RandomUtil.Chance(playChance);
        }

        public void Play(AudioSource source, float spacialBlend, Vector3 position = default, float delay = 0f)
        {
            SetupAudioSource(source, spacialBlend, position, out float finalVolume, out float baseDelay);
            float totalDelay = baseDelay + delay;
            source.loop = false;
            source.volume = finalVolume;
            if (totalDelay > 0f)
            {
                source.PlayDelayed(totalDelay);
            }
            else
            {
                source.Play();
            }
        }

        public SoundLoopHandle PlayLooped(AudioSource source, float spacialBlend, Vector3 position = default, float duration = 0f, Func<float, float> ease = null)
        {
            SetupAudioSource(source, spacialBlend, position, out float finalVolume, out float delay);
            source.loop = true;
            if (duration <= 0f)
            {
                source.volume = finalVolume;
                if (delay >= 0)
                {
                    source.PlayDelayed(delay);
                }
                else
                {
                    source.Play();
                }
                return new SoundLoopHandle(source);
            }
            else
            {
                source.volume = 0f;
                Coroutine coroutine = source.FadeToDelayed(delay, finalVolume, duration, ease);
                return new SoundLoopHandle(source, coroutine);
            }
        }

        public void StopLooped(SoundLoopHandle handle, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (handle.Coroutine != null)
            {
                CoroutineUtil.StopCoroutine(handle.Coroutine);
            }
            if (handle.Source != null)
            {

                if (fadeDuration <= 0f)
                {
                    handle.Source.Stop();
                }
                else
                {
                    handle.Source.FadeTo(0f, fadeDuration, fadeEase);
                }
            }
        }

        private void SetupAudioSource(AudioSource source, float spacialBlend, Vector3 position, out float finalVolume, out float delay)
        {
            Sound sound = sounds.SelectByWeight(s => s.Weight);
            source.clip = sound.Clip;
            source.transform.position = position;
            source.priority = priority;
            source.outputAudioMixerGroup = mixerGroup;
            source.pitch = pitch * sound.Pitch;
            source.spatialBlend = spacialBlend;
            source.minDistance = distance.Min;
            source.maxDistance = distance.Max;
            source.rolloffMode = rolloffMode;
            source.playOnAwake = false;
            if (source.rolloffMode == AudioRolloffMode.Custom) source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloff);
            finalVolume = volume * sound.Volume;
            delay = sound.Delay;
        }
    }
}