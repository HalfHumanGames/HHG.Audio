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
        [Flags]
        public enum FilterFlags
        {
            None = 0,
            LowPass = 1 << 0,
            HighPass = 1 << 1,
            Echo = 1 << 2,
            Chorus = 1 << 3,
            Distortion = 1 << 4,
            Reverb = 1 << 5
        }

        public bool IsLoaded => LoadedCount == sounds.Count;
        public int LoadedCount => sounds.Count(sound => sound.IsLoaded);
        public List<Sound> Sounds => sounds;
        public bool HasLayers => hasLayers;
        public IReadOnlyList<SoundGroupAsset> LayerGroups => layerGroups;

        public event Action<SoundGroupAsset> Loaded;

        private bool UsesLowPass => (filters & FilterFlags.LowPass) != 0;
        private bool UsesHighPass => (filters & FilterFlags.HighPass) != 0;
        private bool UsesEcho => (filters & FilterFlags.Echo) != 0;
        private bool UsesChorus => (filters & FilterFlags.Chorus) != 0;
        private bool UsesDistortion => (filters & FilterFlags.Distortion) != 0;
        private bool UsesReverb => (filters & FilterFlags.Reverb) != 0;

        [EditorButton(nameof(TestPlay), "Test Play", PositionType = ButtonPositionType.Above)]
        [EditorButton(nameof(TestStop), "Test Stop", PositionType = ButtonPositionType.Above)]
        [SerializeField] private float playChance = 1f;
        [SerializeField] private int priority;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField, Range(Sound.SilentDb, 0f)] private float volumeDb = 0f;
        [SerializeField] private float pitchSemitones = 0f;
        [SerializeField] private float cooldown = .2f;
        [SerializeField] private int maxVoices = 5;
        [SerializeField] private float fadeInDuration = 0f;
        [SerializeField] private float fadeOutDuration = 0f;
        [SerializeField, Row] private MinMaxFloat distance = new MinMaxFloat(1f, 500f);
        [SerializeField] private AudioRolloffMode rolloffMode;
        [SerializeField] private AnimationCurve customRolloff;

        [SerializeField] private FilterFlags filters;
        [SerializeField, ShowIf(nameof(UsesLowPass), true)] private LowPassFilterSettings lowPass = new LowPassFilterSettings();
        [SerializeField, ShowIf(nameof(UsesHighPass), true)] private HighPassFilterSettings highPass = new HighPassFilterSettings();
        [SerializeField, ShowIf(nameof(UsesEcho), true)] private EchoFilterSettings echo = new EchoFilterSettings();
        [SerializeField, ShowIf(nameof(UsesChorus), true)] private ChorusFilterSettings chorus = new ChorusFilterSettings();
        [SerializeField, ShowIf(nameof(UsesDistortion), true)] private DistortionFilterSettings distortion = new DistortionFilterSettings();
        [SerializeField, ShowIf(nameof(UsesReverb), true)] private ReverbFilterSettings reverb = new ReverbFilterSettings();

        [SerializeField] private List<Sound> sounds = new List<Sound>();
        [SerializeField] private bool hasLayers;
        [SerializeField, ShowIf(nameof(hasLayers), true)] private List<SoundGroupAsset> layerGroups = new List<SoundGroupAsset>();

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

            if (hasLayers)
            {
                foreach (SoundGroupAsset layerGroup in layerGroups)
                {
                    layerGroup.Load();
                }
            }
        }

        private void TestPlay()
        {
            Sound.Play(this);
        }

        private void TestStop()
        {
            Sound.Stop(this);
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

        public Coroutine Play(AudioSource source, float spacialBlend, Vector3 position = default, float delay = 0f)
        {
            SetupAudioSource(source, spacialBlend, position, out float finalVolume, out float baseDelay);
            float totalDelay = baseDelay + delay;
            source.loop = false;

            if (fadeInDuration <= 0f)
            {
                source.volume = finalVolume;
                if (totalDelay > 0f)
                {
                    source.PlayDelayed(totalDelay);
                }
                else
                {
                    source.Play();
                }
                return null;
            }
            else
            {
                source.volume = 0f;
                return source.FadeToDelayed(totalDelay, finalVolume, fadeInDuration);
            }
        }

        public Coroutine Stop(AudioSource source)
        {
            if (fadeOutDuration <= 0f)
            {
                source.Stop();
                return null;
            }
            else
            {
                return source.FadeTo(0f, fadeOutDuration);
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
            source.pitch = Sound.SemitonesToPitch(pitchSemitones + sound.PitchOffsetSemitones);
            source.spatialBlend = spacialBlend;
            source.minDistance = distance.Min;
            source.maxDistance = distance.Max;
            source.rolloffMode = rolloffMode;
            source.playOnAwake = false;
            if (source.rolloffMode == AudioRolloffMode.Custom) source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customRolloff);
            ApplyFilters(source);
            finalVolume = Mathf.Clamp01(Sound.DecibelsToLinear(volumeDb + sound.VolumeOffsetDb));
            delay = sound.DelayOffsetSeconds;
        }

        private void ApplyFilters(AudioSource source)
        {
            lowPass.Apply(source, UsesLowPass);
            highPass.Apply(source, UsesHighPass);
            echo.Apply(source, UsesEcho);
            chorus.Apply(source, UsesChorus);
            distortion.Apply(source, UsesDistortion);
            reverb.Apply(source, UsesReverb);
        }

        private void OnValidate()
        {
            foreach(Sound sound in sounds)
            {
                sound.Load(true);
            }
        }
    }
}
