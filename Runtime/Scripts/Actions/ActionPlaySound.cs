using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [Serializable]
    public class ActionPlaySound : IAction
    {
        [SerializeField, Dropdown] private SoundGroupAsset sound;
        [SerializeField] private bool playAtTransform;

        public void Invoke(MonoBehaviour invoker)
        {
            if (playAtTransform)
            {
                Sound.Play(sound, invoker.transform.position);
            }
            else
            {
                Sound.Play(sound);
            }
        }
    }
}