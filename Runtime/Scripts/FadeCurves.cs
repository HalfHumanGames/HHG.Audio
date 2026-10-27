using System;
using UnityEngine;

namespace HHG.Audio.Runtime
{
    // Equal-power curves keep perceived loudness changing evenly across the fade
    internal static class FadeCurves
    {
        public static readonly Func<float, float> In = t => Mathf.Sin(t * Mathf.PI * .5f);
        public static readonly Func<float, float> Out = t => 1f - Mathf.Cos(t * Mathf.PI * .5f);
    }
}
