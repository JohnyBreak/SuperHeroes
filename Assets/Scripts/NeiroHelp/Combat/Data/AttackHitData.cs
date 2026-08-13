using System;
using UnityEngine;

namespace NeiroHelp.Combat
{
    [Serializable]
    public class AttackHitData
    {
        public bool Enabled = true;
        public HitProbeMode Mode = HitProbeMode.SweepSocket;
        public HitCastShape Shape = HitCastShape.Box;

        [Tooltip("Half extents for box cast/overlap.")]
        public Vector3 BoxHalfExtents = new Vector3(0.15f, 0.15f, 0.15f);

        [Tooltip("Radius for sphere/capsule.")]
        public float Radius = 0.2f;

        [Tooltip("Capsule height (including hemispheres).")]
        public float CapsuleHeight = 0.5f;

        [Tooltip("Used by OverlapAtOffset — offset in attacker local space.")]
        public Vector3 LocalOffset = new Vector3(0f, 1f, 1f);

        public LayerMask HitMask = ~0;
        public float Damage = 10f;
        public float KnockbackForce = 4f;
        public Vector3 KnockbackLocalDirection = new Vector3(0f, 0.15f, 1f);
        public bool HitSameTargetOncePerAttack = true;
    }
}
