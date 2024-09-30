using HHG.Common.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace HHG.Audio.Runtime
{
    [CreateAssetMenu(fileName = "Sfx Group", menuName = "HHG/Audio/Sfx Group")]
    public class SfxGroupAsset : StringNameAsset
    {
        public bool IsLoaded => LoadedCount == sfxs.Count;
        public int LoadedCount => sfxs.Count(sfx => sfx.IsLoaded);
        public List<Sfx> Sfxs => sfxs;

        public event Action<SfxGroupAsset> Loaded;

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
        [SerializeField] private List<Sfx> sfxs = new List<Sfx>();

        public void Load()
        {
            if (!IsLoaded)
            {
                foreach (Sfx sfx in sfxs)
                {
                    sfx.Loaded += OnLoaded;
                    sfx.Load();
                }
            }
        }

        private void OnLoaded(Sfx sfx)
        {
            sfx.Loaded -= OnLoaded;

            if (IsLoaded)
            {
                Loaded?.Invoke(this);
            }
        }

        public bool CanPlay(int voiceCount, float timestamp)
        {
            return voiceCount < maxVoices && Time.unscaledTime - timestamp > cooldown && RandomUtil.Chance(playChance);
        }

        public void Play(AudioSource source, float spacialBlend, Vector3 position = default)
        {
            SetupAudioSource(source, spacialBlend, position, out float finalVolume, out float delay);
            source.loop = false;
            source.volume = finalVolume;
            if (delay >= 0f)
            {
                source.PlayDelayed(delay);
            }
            else
            {
                source.Play();
            }
        }

        public SfxLoopHandle PlayLooped(AudioSource source, float spacialBlend, Vector3 position = default, float duration = 0f, Func<float, float> ease = null)
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
                return new SfxLoopHandle(source);
            }
            else
            {
                source.volume = 0f;
                Coroutine coroutine = source.FadeToDelayed(delay, finalVolume, duration, ease);
                return new SfxLoopHandle(source, coroutine);
            }
        }

        public void StopLooped(SfxLoopHandle handle, float fadeDuration = 0f, Func<float, float> fadeEase = null)
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
            Sfx sfx = sfxs.SelectByWeight(s => s.Weight);
            source.clip = sfx.Clip;
            source.transform.position = position;
            source.priority = priority;
            source.outputAudioMixerGroup = mixerGroup;
            source.pitch = pitch * sfx.Pitch;
            source.spatialBlend = spacialBlend;
            source.minDistance = distance.Min;
            source.maxDistance = distance.Max;
            source.rolloffMode = rolloffMode;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloff);
            finalVolume = volume * sfx.Volume;
            delay = sfx.Delay;
        }
    }
}