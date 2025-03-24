using UnityEngine;
using HHG.Common.Runtime;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace HHG.Audio.Runtime
{
    public partial class Sound
    {
        public enum Space
        {
            _2D,
            _3D
        }

        private static GameObject _sound;
        private static GameObject sound
        {
            get
            {
                if (_sound == null && !isQuitting)
                {
                    _sound = new GameObject(nameof(Sound));
                    Object.DontDestroyOnLoad(_sound);
                }
                return _sound;
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
                    _pool = new LinkedPool<AudioSource>(CreateAudioSource, OnGetAudioSource, OnReleaseAudioSource, OnDestroyAudioSource, false, voices);
                }

                return _pool;
            }
        }

        private static List<AudioSource> activeSources = new List<AudioSource>();
        private static Dictionary<AudioSource, SoundGroupAsset> sourceToGroupMap = new Dictionary<AudioSource, SoundGroupAsset>();
        private static Dictionary<SoundGroupAsset, List<SoundLoopHandle>> groupToHandlesMap = new Dictionary<SoundGroupAsset, List<SoundLoopHandle>>();
        private static Dictionary<SoundGroupAsset, int> voiceCounts = new Dictionary<SoundGroupAsset, int>();
        private static Dictionary<SoundGroupAsset, float> timestamps = new Dictionary<SoundGroupAsset, float>();
        private static Coroutine coroutine;
        private static bool isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Initialize()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;
#if UNITY_EDITOR
            EditorApplication.quitting -= OnApplicationQuit;
            EditorApplication.quitting += OnApplicationQuit;
#endif
        }

        private static void OnSceneUnloaded(Scene scene)
        {
            foreach (AudioSource activeSource in activeSources)
            {
                pool.Release(activeSource);
            }

            activeSources.Clear();
            voiceCounts.Clear();
            timestamps.Clear();
            coroutine = null;
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
                        SoundGroupAsset group = sourceToGroupMap[source];
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
            go.transform.SetParent(sound.transform);
            go.SetActive(false);
            return go.AddComponent<AudioSource>();
        }

        private static void OnGetAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
        }

        private static void OnReleaseAudioSource(AudioSource source)
        {
            source.gameObject.SetActive(false);
        }

        private static void OnDestroyAudioSource(AudioSource source)
        {
            Object.Destroy(source.gameObject);
        }

        private static void OnApplicationQuit()
        {
            isQuitting = true;
        }

        public static void Play(string groupName)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                PlayInternal(group, Space._2D);
            }
        }

        public static void Play(string groupName, Vector3 position)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                PlayInternal(group, Space._3D, position);
            }
        }

        public static void Play(SoundGroupAsset group)
        {
            if (!isQuitting && group != null)
            {
                PlayInternal(group, Space._2D);
            }
        }

        public static void Play(SoundGroupAsset group, Vector3 position)
        {
            if (!isQuitting && group != null)
            {
                PlayInternal(group, Space._3D, position);
            }
        }

        public static void PlayLooped(string groupName, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                PlayInternal(group, Space._2D, default, true, fadeDuration, fadeEase);
            }
        }

        public static void PlayLooped(string groupName, Vector3 position, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                PlayInternal(group, Space._3D, position, true, fadeDuration, fadeEase);
            }
        }

        public static void PlayLooped(SoundGroupAsset group, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && group != null)
            {
                PlayInternal(group, Space._2D, default, true, fadeDuration, fadeEase);
            }
        }

        public static void PlayLooped(SoundGroupAsset group, Vector3 position, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && group != null)
            {
                PlayInternal(group, Space._3D, position, true, fadeDuration, fadeEase);
            }
        }

        public static void StopLooped(string groupName, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                StopInternal(group, fadeDuration, fadeEase);
            }
        }

        public static void StopLooped(SoundGroupAsset group, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (!isQuitting && group != null)
            {
                StopInternal(group, fadeDuration, fadeEase);
            }
        }

        private static void PlayInternal(SoundGroupAsset group, Space space, Vector3 position = default, bool loop = false, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (group.IsLoaded)
            {
                PlayInternalNow(group, space, position, loop, fadeDuration, fadeEase);
            }
            else
            {
                group.Loaded += group =>
                {
                    PlayInternalNow(group, space, position, loop, fadeDuration, fadeEase);
                };

                group.Load();
            }
        }

        private static void PlayInternalNow(SoundGroupAsset group, Space space, Vector3 position, bool loop, float fadeDuration, Func<float, float> fadeEase)
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
                timestamps[group] = Time.unscaledTime;

                if (loop)
                {
                    SoundLoopHandle loopHandle = group.PlayLooped(source, (float)space, position, fadeDuration, fadeEase);

                    if (!groupToHandlesMap.ContainsKey(group))
                    {
                        groupToHandlesMap.Add(group, new List<SoundLoopHandle>());
                    }

                    groupToHandlesMap[group].Add(loopHandle);
                }
                else
                {
                    group.Play(source, (float)space, position);
                }
            }
        }

        private static void StopInternal(SoundGroupAsset group, float fadeDuration = 0f, Func<float, float> fadeEase = null)
        {
            if (groupToHandlesMap.TryGetValue(group, out List<SoundLoopHandle> handles) && handles.Count > 0)
            {
                int last = handles.Count - 1;
                group.StopLooped(handles[last], fadeDuration, fadeEase);
                handles.RemoveAt(last);
            }
        }
    }
}