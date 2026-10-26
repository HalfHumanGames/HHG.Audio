using HHG.Common.Runtime;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace HHG.Audio.Runtime
{
    [System.Serializable]
    [MovedFrom(true, "Dungeonspire", "Assembly-CSharp", "PlaySound")]
    public class PlaySound : MetaBehaviour
    {
        [SerializeField] private SoundGroupAsset soundGroup;

        public override void Start()
        {
            Sound.Play(soundGroup);
        }
    }
}
