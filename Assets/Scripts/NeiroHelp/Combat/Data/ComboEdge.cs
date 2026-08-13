using System;

namespace NeiroHelp.Combat
{
    [Serializable]
    public class ComboEdge
    {
        public string FromAttackId;
        public string ToAttackId;
        public InputChord Chord;
        public AttackContext Context = AttackContext.Both;
        public int Priority;
        public bool RequireHitConfirm;
    }
}
