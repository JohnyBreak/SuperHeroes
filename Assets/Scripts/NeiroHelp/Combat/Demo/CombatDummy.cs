using UnityEngine;

namespace NeiroHelp.Combat
{
    /// <summary>
    /// Simple hurtbox + knockback receiver for combat demos.
    /// </summary>
    public class CombatDummy : MonoBehaviour, ICombatHurtbox
    {
        [SerializeField] private float _knockbackDamping = 5f;
        [SerializeField] private float _mass = 1f;

        private Vector3 _velocity;
        private float _lastDamage;

        public float LastDamage => _lastDamage;

        public void ReceiveHit(in CombatHitInfo hitInfo)
        {
            _lastDamage = hitInfo.Damage;
            _velocity += hitInfo.WorldKnockback / Mathf.Max(0.01f, _mass);
            Debug.Log($"{name} hit for {hitInfo.Damage}, knockback {hitInfo.WorldKnockback}");
        }

        private void Update()
        {
            if (_velocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            transform.position += _velocity * Time.deltaTime;
            _velocity = Vector3.Lerp(_velocity, Vector3.zero, _knockbackDamping * Time.deltaTime);
        }
    }
}
