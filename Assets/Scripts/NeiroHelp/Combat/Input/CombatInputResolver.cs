using UnityEngine;

namespace NeiroHelp.Combat
{
    public class CombatInputResolver
    {
        private readonly ComboGraphDefinition _graph;
        private readonly CombatInputSampler _sampler;
        private readonly CombatTargeting _targeting;

        public CombatInputResolver(
            ComboGraphDefinition graph,
            CombatInputSampler sampler,
            CombatTargeting targeting)
        {
            _graph = graph;
            _sampler = sampler;
            _targeting = targeting;
        }

        public void Tick(float deltaTime, Vector2 moveInput)
        {
            AimDirection aim = _targeting.ResolveAim(moveInput);
            _sampler.Tick(deltaTime, aim);
        }

        public bool TryResolveStarter(AttackContext context, out AttackDefinition attack)
        {
            attack = null;
            if (!_sampler.TryConsumeBufferedChord(out InputChord chord))
            {
                return false;
            }

            return _graph.TryResolve(
                fromAttackId: null,
                chord: chord,
                context: context,
                hadHitConfirm: false,
                out attack);
        }

        public bool TryResolveNext(
            string currentAttackId,
            AttackContext context,
            bool hadHitConfirm,
            out AttackDefinition attack)
        {
            attack = null;
            if (!_sampler.TryConsumeBufferedChord(out InputChord chord))
            {
                return false;
            }

            if (_graph.TryResolve(currentAttackId, chord, context, hadHitConfirm, out attack))
            {
                return true;
            }

            // Put chord back is not supported; starter edges from null can still open a new string later.
            return false;
        }

        public void ClearBuffer()
        {
            _sampler.ClearBuffer();
        }
    }
}
