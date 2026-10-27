using UnityEngine;
using HHG.Common.Runtime;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        // Includes instances still waiting for their group to load (no source yet)
        private static List<SoundInstance> activeInstances = new List<SoundInstance>();
        private static Dictionary<SoundGroupAsset, int> voiceCounts = new Dictionary<SoundGroupAsset, int>();
        private static Dictionary<SoundGroupAsset, float> timestamps = new Dictionary<SoundGroupAsset, float>();
        private static HashSet<SoundGroupAsset> layeringInProgress = new HashSet<SoundGroupAsset>();
        private static Coroutine coroutine;
        private static bool isQuitting;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Unity Editor retains state across play and edit modes
            // so we clear all collections to prevent stale caches
            activeInstances.Clear();
            voiceCounts.Clear();
            timestamps.Clear();
            layeringInProgress.Clear();

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
            for (int i = activeInstances.Count - 1; i >= 0; i--)
            {
                ReleaseInstance(activeInstances[i]);
            }

            activeInstances.Clear();
            voiceCounts.Clear();
            timestamps.Clear();
            layeringInProgress.Clear();
            coroutine = null;
        }

        private static IEnumerator CheckIfAudioSourceIsDonePlaying()
        {
            while (true)
            {
                for (int i = activeInstances.Count - 1; i >= 0; i--)
                {
                    SoundInstance instance = activeInstances[i];

                    if (instance.Source != null)
                    {
                        if (!instance.Source.isPlaying)
                        {
                            ReleaseInstance(instance);
                        }
                    }
                    else if (instance.LoadedHandler != null)
                    {
                        // A clip failed to load (the error is already logged), so the group will never finish loading
                        if (!instance.Group.IsLoaded && !instance.Group.IsLoading)
                        {
                            ReleaseInstance(instance);
                        }
                    }
                    // Groups with only layers have no source, so they last as long as their layers
                    else if (!instance.HasActiveLayers)
                    {
                        ReleaseInstance(instance);
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

        public static SoundHandle Play(string groupName)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? Play(group) : default;
        }

        public static SoundHandle Play(string groupName, Vector3 position)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? Play(group, position) : default;
        }

        public static SoundHandle Play(SoundGroupAsset group)
        {
            return PlayInternal(group, Space._2D);
        }

        public static SoundHandle Play(SoundGroupAsset group, Vector3 position)
        {
            return PlayInternal(group, Space._3D, position);
        }

        public static SoundHandle PlayDelayed(string groupName, float delay)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? PlayDelayed(group, delay) : default;
        }

        public static SoundHandle PlayDelayed(string groupName, float delay, Vector3 position)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? PlayDelayed(group, delay, position) : default;
        }

        public static SoundHandle PlayDelayed(SoundGroupAsset group, float delay)
        {
            return PlayInternal(group, Space._2D, delay: delay);
        }

        public static SoundHandle PlayDelayed(SoundGroupAsset group, float delay, Vector3 position)
        {
            return PlayInternal(group, Space._3D, position, delay: delay);
        }

        public static SoundHandle PlayLooped(string groupName, float fadeDuration = -1f)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? PlayLooped(group, fadeDuration) : default;
        }

        public static SoundHandle PlayLooped(string groupName, Vector3 position, float fadeDuration = -1f)
        {
            return !isQuitting && Database.TryGet(groupName, out SoundGroupAsset group) ? PlayLooped(group, position, fadeDuration) : default;
        }

        public static SoundHandle PlayLooped(SoundGroupAsset group, float fadeDuration = -1f)
        {
            return PlayInternal(group, Space._2D, default, true, 0f, fadeDuration);
        }

        public static SoundHandle PlayLooped(SoundGroupAsset group, Vector3 position, float fadeDuration = -1f)
        {
            return PlayInternal(group, Space._3D, position, true, 0f, fadeDuration);
        }

        public static void Stop(SoundHandle handle, float fadeDuration = -1f)
        {
            if (!isQuitting && handle.TryGetInstance(out SoundInstance instance))
            {
                StopInstance(instance, fadeDuration);
            }
        }

        public static void Stop(string groupName, float fadeDuration = -1f)
        {
            if (!isQuitting && Database.TryGet(groupName, out SoundGroupAsset group))
            {
                Stop(group, fadeDuration);
            }
        }

        public static void Stop(SoundGroupAsset group, float fadeDuration = -1f)
        {
            if (!isQuitting && group != null)
            {
                // Stopping an instance also stops its layers, which can remove other entries from the list
                using (Pool.GetList(out List<SoundHandle> handles))
                {
                    foreach (SoundInstance instance in activeInstances)
                    {
                        if (instance.Group == group)
                        {
                            handles.Add(new SoundHandle(instance));
                        }
                    }

                    foreach (SoundHandle handle in handles)
                    {
                        Stop(handle, fadeDuration);
                    }
                }
            }
        }

        private static SoundHandle PlayInternal(SoundGroupAsset group, Space space, Vector3 position = default, bool loop = false, float delay = 0f, float fadeDuration = -1f)
        {
            if (isQuitting || group == null)
            {
                return default;
            }

            SoundInstance instance = CreateInstance(group, space, position, loop, delay, fadeDuration);
            return instance != null ? new SoundHandle(instance) : default;
        }

        private static SoundInstance CreateInstance(SoundGroupAsset group, Space space, Vector3 position, bool loop, float delay, float fadeDuration)
        {
            // Guards against layer groups that form a cycle (including self-reference)
            if (!layeringInProgress.Add(group))
            {
                return null;
            }

            SoundInstance instance = null;

            if (group.IsLoaded)
            {
                if (CanPlay(group))
                {
                    instance = GetInstance(group, loop);
                    StartInstance(instance, space, position, delay, fadeDuration);
                }
            }
            else
            {
                instance = GetInstance(group, loop);
                SoundHandle handle = new SoundHandle(instance);
                instance.LoadedHandler = _ => OnInstanceGroupLoaded(handle, space, position, delay, fadeDuration);
                group.Loaded += instance.LoadedHandler;
                group.Load();

                // The load can complete immediately and release the instance if it can't play
                if (!handle.IsValid)
                {
                    instance = null;
                }
            }

            if (instance != null && group.HasLayers)
            {
                foreach (SoundGroupAsset layerGroup in group.LayerGroups)
                {
                    SoundInstance layerInstance = CreateInstance(layerGroup, space, position, loop, delay, fadeDuration);

                    if (layerInstance != null)
                    {
                        instance.Layers.Add(new SoundHandle(layerInstance));
                    }
                }
            }

            layeringInProgress.Remove(group);
            return instance;
        }

        private static void OnInstanceGroupLoaded(SoundHandle handle, Space space, Vector3 position, float delay, float fadeDuration)
        {
            if (!handle.TryGetInstance(out SoundInstance instance))
            {
                return;
            }

            instance.Group.Loaded -= instance.LoadedHandler;
            instance.LoadedHandler = null;

            if (CanPlay(instance.Group))
            {
                StartInstance(instance, space, position, delay, fadeDuration);
            }
            else
            {
                StopInstance(instance);
            }
        }

        private static bool CanPlay(SoundGroupAsset group)
        {
            voiceCounts.TryGetValue(group, out int voiceCount);
            timestamps.TryGetValue(group, out float timestamp);
            return group.CanPlay(voiceCount, timestamp);
        }

        private static SoundInstance GetInstance(SoundGroupAsset group, bool loop)
        {
            // Started here rather than when a sound plays so instances waiting on a load also get checked
            if (coroutine == null)
            {
                coroutine = CoroutineUtil.StartCoroutine(CheckIfAudioSourceIsDonePlaying());
            }

            SoundInstance instance = Pool.Get<SoundInstance>();
            instance.Group = group;
            instance.IsLooping = loop;
            activeInstances.Add(instance);
            return instance;
        }

        private static void StartInstance(SoundInstance instance, Space space, Vector3 position, float delay, float fadeDuration)
        {
            SoundGroupAsset group = instance.Group;
            timestamps[group] = Time.unscaledTime;

            // Groups with only layers have nothing of their own to play
            if (!group.HasSounds)
            {
                return;
            }

            AudioSource source = pool.Get();
            instance.Source = source;
            voiceCounts.TryGetValue(group, out int voiceCount);
            voiceCounts[group] = voiceCount + 1;

            instance.Fade = instance.IsLooping ?
                group.PlayLooped(source, (float)space, position, fadeDuration) :
                group.Play(source, (float)space, position, delay);
        }

        private static void StopInstance(SoundInstance instance, float fadeDuration = -1f)
        {
            foreach (SoundHandle layer in instance.Layers)
            {
                if (layer.TryGetInstance(out SoundInstance layerInstance))
                {
                    StopInstance(layerInstance, fadeDuration);
                }
            }

            instance.Layers.Clear();

            // Still waiting for its group to load, or has only layers, so there is nothing to fade out
            if (instance.Source == null)
            {
                ReleaseInstance(instance);
                return;
            }

            CoroutineUtil.StopCoroutine(instance.Fade);
            instance.IsStopping = true;
            instance.Fade = instance.Group.Stop(instance.Source, fadeDuration);

            if (instance.Fade == null)
            {
                ReleaseInstance(instance);
            }
        }

        private static void ReleaseInstance(SoundInstance instance)
        {
            // Already released
            if (instance.Group == null)
            {
                return;
            }

            if (instance.LoadedHandler != null)
            {
                instance.Group.Loaded -= instance.LoadedHandler;
            }

            CoroutineUtil.StopCoroutine(instance.Fade);

            if (instance.Source != null)
            {
                voiceCounts[instance.Group]--;
                pool.Release(instance.Source);
            }

            activeInstances.Remove(instance);
            instance.Reset();
            Pool.Release(instance);
        }
    }
}
