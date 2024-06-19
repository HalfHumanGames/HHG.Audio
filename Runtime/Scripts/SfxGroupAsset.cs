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

        [SerializeField] private float playChance = 1;
        [SerializeField] private int priority;
        [SerializeField] private AudioMixerGroup mixerGroup;
        [SerializeField] private float volume = 1;
        [SerializeField] private float pitch = 1;
        [SerializeField] private float cooldown;
        [SerializeField] private int maxVoices = 5;
        [SerializeField, Row] private MinMaxFloat distance = new MinMaxFloat(1, 500);
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
            return voiceCount < maxVoices && Time.time - timestamp > cooldown && RandomUtil.Chance(playChance);
        }

        public void Play(AudioSource source, float spacialBlend, Vector3 position = default)
        {
            Sfx sfx = sfxs.SelectByWeight(s => s.Weight);
            source.transform.position = position;
            source.priority = priority;
            source.outputAudioMixerGroup = mixerGroup;
            source.volume = volume * sfx.Volume;
            source.pitch = pitch * sfx.Pitch;
            source.spatialBlend = spacialBlend;
            source.minDistance = distance.Min;
            source.maxDistance = distance.Max;
            source.loop = false; //sfx.Loop;
            source.clip = sfx.Clip;
            source.PlayDelayed(sfx.Delay);
        }
    }
}