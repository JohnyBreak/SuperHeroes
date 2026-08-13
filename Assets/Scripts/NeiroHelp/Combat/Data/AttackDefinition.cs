using System.Collections.Generic;
using UnityEngine;

namespace NeiroHelp.Combat
{
    [CreateAssetMenu(menuName = "NeiroHelp/Combat/Attack Definition", fileName = "AttackDefinition")]
    public class AttackDefinition : ScriptableObject
    {
        public string Id = "Attack";
        public AttackContext Context = AttackContext.Ground;
        public float DurationSeconds = 0.45f;
        public AttackPhaseWindows Phases = AttackPhaseWindows.DefaultPunch;
        public AttackMotionData Motion = new AttackMotionData();
        public AttackHitData Hit = new AttackHitData();
        public List<AttackEffectEntry> Effects = new List<AttackEffectEntry>();

        public bool SupportsContext(AttackContext runtimeContext)
        {
            return Context == AttackContext.Both || Context == runtimeContext;
        }
    }
}
