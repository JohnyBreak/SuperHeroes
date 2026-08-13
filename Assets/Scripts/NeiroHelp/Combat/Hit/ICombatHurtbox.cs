using UnityEngine;

namespace NeiroHelp.Combat
{
    public readonly struct CombatHitInfo
    {
        public readonly Collider Collider;
        public readonly Vector3 Point;
        public readonly Vector3 Normal;
        public readonly float Damage;
        public readonly Vector3 WorldKnockback;

        public CombatHitInfo(
            Collider collider,
            Vector3 point,
            Vector3 normal,
            float damage,
            Vector3 worldKnockback)
        {
            Collider = collider;
            Point = point;
            Normal = normal;
            Damage = damage;
            WorldKnockback = worldKnockback;
        }
    }

    public interface ICombatHurtbox
    {
        void ReceiveHit(in CombatHitInfo hitInfo);
    }
}
