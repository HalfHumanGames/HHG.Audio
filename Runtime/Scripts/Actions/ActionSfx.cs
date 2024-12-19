using HHG.Common.Runtime;
using System;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    [Serializable]
    public class ActionSfx : IAction
    {
        [SerializeField, Dropdown] private SfxGroupAsset sfx;
        [SerializeField] private bool playAtTransform;

        public void Invoke(MonoBehaviour invoker)
        {
            if (playAtTransform)
            {
                Sfx.Play(sfx, invoker.transform.position);
            }
            else
            {
                Sfx.Play(sfx);
            }
        }
    }
}