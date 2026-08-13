using UnityEngine;

namespace NeiroHelp.Combat
{
    public class CombatEffectRunner
    {
        private readonly Transform _attacker;
        private readonly CombatTargeting _targeting;

        public CombatEffectRunner(Transform attacker, CombatTargeting targeting)
        {
            _attacker = attacker;
            _targeting = targeting;
        }

        public void Run(AttackEffectEntry entry, AttackDefinition attack)
        {
            if (entry == null || entry.Type == CombatEffectType.None)
            {
                return;
            }

            switch (entry.Type)
            {
                case CombatEffectType.Knockback:
                    // Per-hit knockback is applied from AttackHitData on contact.
                    break;
                case CombatEffectType.LaunchUp:
                    Debug.Log($"[Combat] LaunchUp from {attack.Id}, impulse={entry.FloatParameter}");
                    break;
                case CombatEffectType.SlamDown:
                    Debug.Log($"[Combat] SlamDown from {attack.Id}");
                    break;
                case CombatEffectType.SpawnProjectile:
                    if (entry.PrefabParameter != null)
                    {
                        Object.Instantiate(
                            entry.PrefabParameter,
                            _attacker.position + _attacker.forward + Vector3.up,
                            _attacker.rotation);
                    }
                    else
                    {
                        Debug.Log($"[Combat] Projectile from {attack.Id} (no prefab assigned)");
                    }

                    break;
                case CombatEffectType.DashToTarget:
                    Debug.Log($"[Combat] DashToTarget from {attack.Id} -> {_targeting.CurrentTarget}");
                    break;
                case CombatEffectType.PullTargetToSelf:
                    Debug.Log($"[Combat] PullTargetToSelf from {attack.Id} -> {_targeting.CurrentTarget}");
                    break;
                case CombatEffectType.ApplyStun:
                    Debug.Log($"[Combat] Stun from {attack.Id}, duration={entry.FloatParameter}");
                    break;
            }
        }
    }
}
