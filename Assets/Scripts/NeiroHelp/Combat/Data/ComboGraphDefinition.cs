using System.Collections.Generic;
using UnityEngine;

namespace NeiroHelp.Combat
{
    [CreateAssetMenu(menuName = "NeiroHelp/Combat/Combo Graph", fileName = "ComboGraph")]
    public class ComboGraphDefinition : ScriptableObject
    {
        public List<AttackDefinition> Attacks = new List<AttackDefinition>();
        public List<ComboEdge> Edges = new List<ComboEdge>();

        public AttackDefinition GetAttack(string attackId)
        {
            for (int i = 0; i < Attacks.Count; i++)
            {
                AttackDefinition attack = Attacks[i];
                if (attack != null && attack.Id == attackId)
                {
                    return attack;
                }
            }

            return null;
        }

        public bool TryResolve(
            string fromAttackId,
            InputChord chord,
            AttackContext context,
            bool hadHitConfirm,
            out AttackDefinition nextAttack)
        {
            nextAttack = null;
            ComboEdge bestEdge = null;

            for (int i = 0; i < Edges.Count; i++)
            {
                ComboEdge edge = Edges[i];
                if (edge == null)
                {
                    continue;
                }

                if (!SameAttackId(edge.FromAttackId, fromAttackId))
                {
                    continue;
                }

                if (!edge.Chord.Equals(chord))
                {
                    continue;
                }

                if (edge.Context != AttackContext.Both && edge.Context != context)
                {
                    continue;
                }

                if (edge.RequireHitConfirm && !hadHitConfirm)
                {
                    continue;
                }

                if (bestEdge == null || edge.Priority >= bestEdge.Priority)
                {
                    bestEdge = edge;
                }
            }

            if (bestEdge == null)
            {
                return false;
            }

            nextAttack = GetAttack(bestEdge.ToAttackId);
            return nextAttack != null;
        }

        private static bool SameAttackId(string left, string right)
        {
            bool leftEmpty = string.IsNullOrEmpty(left);
            bool rightEmpty = string.IsNullOrEmpty(right);
            if (leftEmpty || rightEmpty)
            {
                return leftEmpty && rightEmpty;
            }

            return string.Equals(left, right);
        }
    }
}
