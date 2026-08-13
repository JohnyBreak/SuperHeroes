using System.Collections.Generic;
using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Preferred melee probe: each active frame casts from previous socket pose to current.
    /// Also supports OverlapAtOffset for AoE.
    /// </summary>
    public class SweptHitProbe
    {
        private readonly HashSet<int> _hitInstanceIds = new HashSet<int>();
        private Vector3 _previousSocketPosition;
        private bool _hasPreviousSocketPosition;
        private readonly Collider[] _overlapBuffer = new Collider[16];
        private readonly RaycastHit[] _castBuffer = new RaycastHit[16];

        public void Reset()
        {
            _hitInstanceIds.Clear();
            _hasPreviousSocketPosition = false;
        }

        public void PrimeSocket(Vector3 socketPosition)
        {
            _previousSocketPosition = socketPosition;
            _hasPreviousSocketPosition = true;
        }

        public int Probe(
            Transform attacker,
            Transform socket,
            AttackHitData hitData,
            List<CombatHitInfo> results)
        {
            results.Clear();
            if (hitData == null || !hitData.Enabled || attacker == null)
            {
                return 0;
            }

            if (hitData.Mode == HitProbeMode.OverlapAtOffset)
            {
                return ProbeOverlap(attacker, hitData, results);
            }

            if (socket == null)
            {
                return 0;
            }

            return ProbeSweep(attacker, socket.position, socket.rotation, hitData, results);
        }

        private int ProbeSweep(
            Transform attacker,
            Vector3 currentPosition,
            Quaternion currentRotation,
            AttackHitData hitData,
            List<CombatHitInfo> results)
        {
            if (!_hasPreviousSocketPosition)
            {
                PrimeSocket(currentPosition);
                return ProbeOverlapAtWorld(attacker, currentPosition, currentRotation, hitData, results);
            }

            Vector3 delta = currentPosition - _previousSocketPosition;
            float distance = delta.magnitude;
            Vector3 origin = _previousSocketPosition;
            _previousSocketPosition = currentPosition;

            if (distance <= 0.0001f)
            {
                return ProbeOverlapAtWorld(attacker, currentPosition, currentRotation, hitData, results);
            }

            Vector3 direction = delta / distance;
            int count = CastAll(
                hitData.Shape,
                origin,
                currentRotation,
                direction,
                distance,
                hitData,
                _castBuffer);

            for (int i = 0; i < count; i++)
            {
                TryAddHit(attacker, hitData, _castBuffer[i].collider, _castBuffer[i].point, _castBuffer[i].normal, results);
            }

            return results.Count;
        }

        private int ProbeOverlap(
            Transform attacker,
            AttackHitData hitData,
            List<CombatHitInfo> results)
        {
            Vector3 worldCenter = attacker.TransformPoint(hitData.LocalOffset);
            return ProbeOverlapAtWorld(attacker, worldCenter, attacker.rotation, hitData, results);
        }

        private int ProbeOverlapAtWorld(
            Transform attacker,
            Vector3 center,
            Quaternion rotation,
            AttackHitData hitData,
            List<CombatHitInfo> results)
        {
            int count = OverlapAll(hitData.Shape, center, rotation, hitData, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                TryAddHit(attacker, hitData, _overlapBuffer[i], center, attacker.forward, results);
            }

            return results.Count;
        }

        private void TryAddHit(
            Transform attacker,
            AttackHitData hitData,
            Collider collider,
            Vector3 point,
            Vector3 normal,
            List<CombatHitInfo> results)
        {
            if (collider == null)
            {
                return;
            }

            if (collider.transform == attacker || collider.transform.IsChildOf(attacker))
            {
                return;
            }

            int instanceId = collider.GetInstanceID();
            if (hitData.HitSameTargetOncePerAttack && !_hitInstanceIds.Add(instanceId))
            {
                return;
            }

            Vector3 localDirection = hitData.KnockbackLocalDirection.sqrMagnitude > 0.0001f
                ? hitData.KnockbackLocalDirection.normalized
                : Vector3.forward;
            Vector3 worldKnockback = attacker.TransformDirection(localDirection) * hitData.KnockbackForce;

            results.Add(new CombatHitInfo(
                collider,
                point,
                normal,
                hitData.Damage,
                worldKnockback));
        }

        private static int CastAll(
            HitCastShape shape,
            Vector3 origin,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            AttackHitData hitData,
            RaycastHit[] buffer)
        {
            switch (shape)
            {
                case HitCastShape.Sphere:
                    return Physics.SphereCastNonAlloc(
                        origin,
                        hitData.Radius,
                        direction,
                        buffer,
                        distance,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
                case HitCastShape.Capsule:
                {
                    GetCapsulePoints(origin, rotation, hitData, out Vector3 pointA, out Vector3 pointB);
                    return Physics.CapsuleCastNonAlloc(
                        pointA,
                        pointB,
                        hitData.Radius,
                        direction,
                        buffer,
                        distance,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
                }
                default:
                    return Physics.BoxCastNonAlloc(
                        origin,
                        hitData.BoxHalfExtents,
                        direction,
                        buffer,
                        rotation,
                        distance,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
            }
        }

        private static int OverlapAll(
            HitCastShape shape,
            Vector3 center,
            Quaternion rotation,
            AttackHitData hitData,
            Collider[] buffer)
        {
            switch (shape)
            {
                case HitCastShape.Sphere:
                    return Physics.OverlapSphereNonAlloc(
                        center,
                        hitData.Radius,
                        buffer,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
                case HitCastShape.Capsule:
                {
                    GetCapsulePoints(center, rotation, hitData, out Vector3 pointA, out Vector3 pointB);
                    return Physics.OverlapCapsuleNonAlloc(
                        pointA,
                        pointB,
                        hitData.Radius,
                        buffer,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
                }
                default:
                    return Physics.OverlapBoxNonAlloc(
                        center,
                        hitData.BoxHalfExtents,
                        buffer,
                        rotation,
                        hitData.HitMask,
                        QueryTriggerInteraction.Collide);
            }
        }

        private static void GetCapsulePoints(
            Vector3 center,
            Quaternion rotation,
            AttackHitData hitData,
            out Vector3 pointA,
            out Vector3 pointB)
        {
            float straight = Mathf.Max(0f, hitData.CapsuleHeight - hitData.Radius * 2f);
            Vector3 up = rotation * Vector3.up;
            Vector3 half = up * (straight * 0.5f);
            pointA = center + half;
            pointB = center - half;
        }
    }
}
