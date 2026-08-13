using System;
using UnityEngine;

namespace NeiroHelp.Combat
{
    [Serializable]
    public class AttackEffectEntry
    {
        public CombatEffectType Type = CombatEffectType.None;

        [Range(0f, 1f)]
        [Tooltip("Normalized time when the effect fires once.")]
        public float FireAtNormalizedTime = 0.25f;

        public float FloatParameter = 0f;
        public Vector3 VectorParameter = Vector3.zero;
        public GameObject PrefabParameter;
    }
}
