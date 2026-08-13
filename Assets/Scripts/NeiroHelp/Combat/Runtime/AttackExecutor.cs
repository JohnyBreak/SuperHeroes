using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeiroHelp.Combat
{
    public class AttackExecutor
    {
        private readonly Transform _attacker;
        private readonly Transform _hitSocket;
        private readonly SweptHitProbe _hitProbe = new SweptHitProbe();
        private readonly AttackMotionApplier _motionApplier = new AttackMotionApplier();
        private readonly List<CombatHitInfo> _hitResults = new List<CombatHitInfo>(8);
        private readonly HashSet<int> _firedEffectIndexes = new HashSet<int>();

        private AttackDefinition _attack;
        private float _elapsedSeconds;
        private bool _hadHitConfirm;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;
        public bool HadHitConfirm => _hadHitConfirm;
        public AttackDefinition CurrentAttack => _attack;
        public float NormalizedTime { get; private set; }
        public Vector3 LastMotionDelta { get; private set; }
        public event Action<CombatHitInfo> HitLanded;
        public event Action<AttackEffectEntry, AttackDefinition> EffectFired;
        public event Action<AttackDefinition> AttackFinished;

        public AttackExecutor(Transform attacker, Transform hitSocket)
        {
            _attacker = attacker;
            _hitSocket = hitSocket;
        }

        public void Start(AttackDefinition attack)
        {
            _attack = attack;
            _elapsedSeconds = 0f;
            NormalizedTime = 0f;
            _hadHitConfirm = false;
            _isPlaying = true;
            LastMotionDelta = Vector3.zero;
            _firedEffectIndexes.Clear();
            _motionApplier.Reset();
            _hitProbe.Reset();
            if (_hitSocket != null)
            {
                _hitProbe.PrimeSocket(_hitSocket.position);
            }
        }

        public void Stop()
        {
            if (!_isPlaying)
            {
                return;
            }

            _isPlaying = false;
            AttackFinished?.Invoke(_attack);
            _attack = null;
        }

        public void Tick(float deltaTime)
        {
            if (!_isPlaying || _attack == null)
            {
                LastMotionDelta = Vector3.zero;
                return;
            }

            _elapsedSeconds += deltaTime;
            float duration = Mathf.Max(0.01f, _attack.DurationSeconds);
            NormalizedTime = Mathf.Clamp01(_elapsedSeconds / duration);

            LastMotionDelta = _motionApplier.EvaluateWorldDelta(
                _attacker,
                _attack.Motion,
                NormalizedTime);

            FireEffects();

            if (_attack.Phases.IsActive(NormalizedTime))
            {
                int hitCount = _hitProbe.Probe(_attacker, _hitSocket, _attack.Hit, _hitResults);
                for (int i = 0; i < hitCount; i++)
                {
                    CombatHitInfo hit = _hitResults[i];
                    _hadHitConfirm = true;
                    ApplyHitToHurtbox(hit);
                    HitLanded?.Invoke(hit);
                }
            }

            if (NormalizedTime >= 1f)
            {
                AttackDefinition finished = _attack;
                _isPlaying = false;
                AttackFinished?.Invoke(finished);
            }
        }

        public bool IsComboWindowOpen()
        {
            return _isPlaying && _attack != null && _attack.Phases.IsComboWindow(NormalizedTime);
        }

        private void FireEffects()
        {
            List<AttackEffectEntry> effects = _attack.Effects;
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                if (_firedEffectIndexes.Contains(i))
                {
                    continue;
                }

                AttackEffectEntry entry = effects[i];
                if (NormalizedTime + 0.0001f < entry.FireAtNormalizedTime)
                {
                    continue;
                }

                _firedEffectIndexes.Add(i);
                EffectFired?.Invoke(entry, _attack);
            }
        }

        private static void ApplyHitToHurtbox(in CombatHitInfo hit)
        {
            ICombatHurtbox hurtbox = hit.Collider.GetComponentInParent<ICombatHurtbox>();
            hurtbox?.ReceiveHit(hit);
        }
    }
}
