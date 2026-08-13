using System;
using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Local-space displacement over normalized time.
    /// Z = forward along facing, Y = up. X is unused for now.
    /// Curves are sampled as cumulative displacement shape; frame delta is applied as movement.
    /// </summary>
    [Serializable]
    public class AttackMotionData
    {
        public AnimationCurve ForwardDisplacement = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public AnimationCurve HeightDisplacement = AnimationCurve.Constant(0f, 1f, 0f);

        [Tooltip("Scales forward (local Z) displacement from the curve.")]
        public float ForwardDistance = 1.2f;

        [Tooltip("Scales vertical (Y) displacement from the curve.")]
        public float HeightDistance = 0f;

        public bool OverrideGravity = true;
    }
}
