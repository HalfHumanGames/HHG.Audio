using UnityEngine;
using HHG.Common.Runtime;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;




#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HHG.Audio.Runtime
{
    public partial class Sfx
    {
        public enum Space
        {
            _2D,
            _3D
        }

        private static GameObject _sfx;
        private static GameObject sfx
        {
            get
            {
                if (_sfx == null && !isQuitting)
                {
                    _sfx = new GameObject(nameof(Sfx));
                    Object.DontDestroyOnLoad(_sfx);
                }
                return _sfx;
            }
        }

        private static LinkedPool<AudioSource> _pool;
        private static LinkedPool<AudioSource> pool
        {
            get
            {
                if (_pool == null && !isQuitting)
                {
                    int voices = AudioSettings.GetConfiguration().numVirtualVoices;
                    _pool = new LinkedPool<AudioSource>(CreateAudioSource, GetAudioSource, ReleaseAudioSource, DestroyAudioSource, false, voices);
                }

                return _pool;
            }
        }

        private static List<AudioSource> activeSources = new List<AudioSource>();
        private static Dictionary<AudioSource, SfxGroupAsset> sourceToGroupMap = new Dictionary<AudioSource, SfxGroupAsset>();
        private static Dictionary<SfxGroupAsset, int> voiceCounts = new Dictionary<SfxGroupAsset, int>();
        private static Dictionary<SfxGroupAsset, float> timestamps = new Dictionary<SfxGroupAsset, float>();
        private static Coroutine coroutine;
        private static bool isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;
#if UNITY_EDITOR
            EditorApplication.quitting -= OnApplicationQuit;
            EditorApplication.quitting += OnApplicationQuit;
#endif
        }

        private static IEnumerator CheckIfAudioSourceIsDonePlaying()
        {
            while (true)
            {
                for (int i = 0; i < activeSources.Count; i++)
                {
                    AudioSource source = activeSources[i];
                    if (!source.isPlaying)
                    {
                        SfxGroupAsset group = sourceToGroupMap[source];
                        activeSources.RemoveAt(i);
                        sourceToGroupMap.Remove(source);
                        voiceCounts[group]--;
                        pool.Release(source);
                        i--;
                    }
                }
                yield return new WaitForEndOfFrame();
            }
        }

        private static AudioSource CreateAudioSource()
        {
            GameObject go = new GameObject(nameof(AudioSource));
            go.transform.SetParent(sfx.transform);
            go.SetActive(false);
            return go.AddComponent<AudioSource>();
        }

        private static void GetAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        private static void ReleaseAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(false);
        }

        private static void DestroyAudioSource(AudioSource source)
        {
            Object.Destroy(source.gameObject);
        }

        private static void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public static void Play(string groupName)
        {
            PlayInternal(Database.Get<SfxGroupAsset>(groupName), Space._2D);
        }

        public static void Play(string groupName, Vector3 position)
        {
            PlayInternal(Database.Get<SfxGroupAsset>(groupName), Space._3D, position);
        }

        public static void Play(SfxGroupAsset group)
        {
            PlayInternal(group, Space._2D);
        }

        private static void PlayInternal(SfxGroupAsset group, Vector3 position)
        {
            PlayInternal(group, Space._3D, position);
        }

        private static void PlayInternal(SfxGroupAsset group, Space space, Vector3 position = default)
        {
            if (group == null)
            {
                return;
            }

            if (group.IsLoaded)
            {
                PlayInternalNow(group, space, position);
            }
            else
            {
                group.Loaded += group =>
                {
                    PlayInternalNow(group, space, position);
                };

                group.Load();
            }
        }

        private static void PlayInternalNow(SfxGroupAsset group, Space space, Vector3 position = default)
        {
            if (coroutine == null)
            {
                coroutine = CoroutineUtil.StartCoroutine(CheckIfAudioSourceIsDonePlaying());
            }

            if (!voiceCounts.TryGetValue(group, out int voiceCount))
            {
                voiceCounts[group] = 0;
            }

            if (!timestamps.TryGetValue(group, out float timestamp))
            {
                timestamps[group] = 0f;
            }

            if (group.CanPlay(voiceCount, timestamp))
            {
                AudioSource source = pool.Get();
                activeSources.Add(source);
                sourceToGroupMap[source] = group;
                voiceCounts[group]++;
                timestamps[group] = Time.time;
                group.Play(source, (float)space, position);
            }
        }
    }
}