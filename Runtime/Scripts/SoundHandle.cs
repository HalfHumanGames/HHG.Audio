namespace HHG.Audio.Runtime
{
    public readonly struct SoundHandle
    {
        public bool IsValid => instance != null && instance.Version == version;
        public bool IsPlaying => IsValid && !instance.IsStopping;
        public bool IsLooping => IsValid && instance.IsLooping;
        public SoundGroupAsset Group => IsValid ? instance.Group : null;

        private readonly SoundInstance instance;
        private readonly int version;

        internal SoundHandle(SoundInstance instance)
        {
            this.instance = instance;
            version = instance.Version;
        }

        public void Stop(float fadeDuration = -1f)
        {
            Sound.Stop(this, fadeDuration);
        }

        internal bool TryGetInstance(out SoundInstance soundInstance)
        {
            soundInstance = IsValid ? instance : null;
            return soundInstance != null;
        }
    }
}
