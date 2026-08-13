using UnityEngine;

namespace NeiroHelp.Combat
{
    public class CombatTargeting
    {
        private readonly Transform _attacker;
        private readonly float _maxDistance;
        private readonly float _coneDot;
        private Transform _currentTarget;

        public CombatTargeting(Transform attacker, float maxDistance = 12f, float coneDegrees = 70f)
        {
            _attacker = attacker;
            _maxDistance = maxDistance;
            _coneDot = Mathf.Cos(coneDegrees * 0.5f * Mathf.Deg2Rad);
        }

        public Transform CurrentTarget => _currentTarget;

        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }

        public AimDirection ResolveAim(Vector2 moveInput)
        {
            if (_currentTarget == null)
            {
                TryFindTargetInFront();
            }

            if (IsAwayInput(moveInput))
            {
                return AimDirection.AwayFromTarget;
            }

            if (IsTowardInput(moveInput))
            {
                return AimDirection.TowardTarget;
            }

            if (IsDownInput(moveInput))
            {
                return AimDirection.Down;
            }

            if (IsUpInput(moveInput))
            {
                return AimDirection.Up;
            }

            if (_currentTarget == null)
            {
                return AimDirection.Neutral;
            }

            Vector3 toTarget = _currentTarget.position - _attacker.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude < 0.0001f)
            {
                return AimDirection.Neutral;
            }

            Vector3 moveWorld = _attacker.forward * moveInput.y + _attacker.right * moveInput.x;
            if (moveWorld.sqrMagnitude < 0.01f)
            {
                return AimDirection.Neutral;
            }

            float alignment = Vector3.Dot(moveWorld.normalized, toTarget.normalized);
            if (alignment >= 0.35f)
            {
                return AimDirection.TowardTarget;
            }

            if (alignment <= -0.35f)
            {
                return AimDirection.AwayFromTarget;
            }

            return AimDirection.Neutral;
        }

        private static bool IsAwayInput(Vector2 moveInput)
        {
            return moveInput.y < -0.5f || Input.GetKey(KeyCode.S);
        }

        private static bool IsTowardInput(Vector2 moveInput)
        {
            return moveInput.y > 0.5f || Input.GetKey(KeyCode.W);
        }

        private static bool IsDownInput(Vector2 moveInput)
        {
            return moveInput.y < -0.85f || Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftControl);
        }

        private static bool IsUpInput(Vector2 moveInput)
        {
            return Input.GetKey(KeyCode.Space);
        }

        private void TryFindTargetInFront()
        {
            Collider[] colliders = Physics.OverlapSphere(_attacker.position, _maxDistance);
            float bestScore = float.MinValue;
            Transform best = null;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null || collider.transform == _attacker || collider.transform.IsChildOf(_attacker))
                {
                    continue;
                }

                if (collider.GetComponentInParent<ICombatHurtbox>() == null)
                {
                    continue;
                }

                Vector3 toTarget = collider.bounds.center - _attacker.position;
                float distance = toTarget.magnitude;
                if (distance < 0.01f || distance > _maxDistance)
                {
                    continue;
                }

                float dot = Vector3.Dot(_attacker.forward, toTarget.normalized);
                if (dot < _coneDot)
                {
                    continue;
                }

                float score = dot * 2f - distance * 0.05f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = collider.transform;
                }
            }

            _currentTarget = best;
        }
    }
}
