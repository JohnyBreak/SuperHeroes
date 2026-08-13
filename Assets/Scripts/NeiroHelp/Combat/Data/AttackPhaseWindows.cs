using System;
using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// All windows use normalized attack time in [0..1].
    /// Example: startup 0-0.2, active 0.2-0.7, recovery 0.7-1.
    /// </summary>
    [Serializable]
    public struct AttackPhaseWindows
    {
        [Range(0f, 1f)] public float StartupEnd;
        [Range(0f, 1f)] public float ActiveEnd;
        [Range(0f, 1f)] public float ComboOpen;
        [Range(0f, 1f)] public float ComboClose;

        public static AttackPhaseWindows DefaultPunch => new AttackPhaseWindows
        {
            StartupEnd = 0.2f,
            ActiveEnd = 0.7f,
            ComboOpen = 0.35f,
            ComboClose = 0.95f
        };

        public bool IsStartup(float normalizedTime) => normalizedTime < StartupEnd;

        public bool IsActive(float normalizedTime) =>
            normalizedTime >= StartupEnd && normalizedTime < ActiveEnd;

        public bool IsRecovery(float normalizedTime) => normalizedTime >= ActiveEnd;

        public bool IsComboWindow(float normalizedTime) =>
            normalizedTime >= ComboOpen && normalizedTime <= ComboClose;
    }
}
