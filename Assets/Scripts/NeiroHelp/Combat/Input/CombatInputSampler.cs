using UnityEngine;

namespace NeiroHelp.Combat
{
    public class CombatInputSampler
    {
        private readonly KeyCode _lightKey;
        private readonly KeyCode _heavyKey;
        private readonly KeyCode _specialKey;
        private readonly float _holdThresholdSeconds;
        private readonly float _bufferSeconds;

        private float _lightHeldSeconds;
        private float _heavyHeldSeconds;
        private float _specialHeldSeconds;
        private bool _lightHoldConsumed;
        private bool _heavyHoldConsumed;
        private bool _specialHoldConsumed;

        private InputChord? _bufferedChord;
        private float _bufferExpireTime;

        public CombatInputSampler(
            KeyCode lightKey = KeyCode.Mouse0,
            KeyCode heavyKey = KeyCode.Mouse1,
            KeyCode specialKey = KeyCode.F,
            float holdThresholdSeconds = 0.35f,
            float bufferSeconds = 0.2f)
        {
            _lightKey = lightKey;
            _heavyKey = heavyKey;
            _specialKey = specialKey;
            _holdThresholdSeconds = holdThresholdSeconds;
            _bufferSeconds = bufferSeconds;
        }

        public void Tick(float deltaTime, AimDirection aim)
        {
            ExpireBuffer();

            SampleButton(
                CombatButton.Light,
                _lightKey,
                ref _lightHeldSeconds,
                ref _lightHoldConsumed,
                aim,
                deltaTime);
            SampleButton(
                CombatButton.Heavy,
                _heavyKey,
                ref _heavyHeldSeconds,
                ref _heavyHoldConsumed,
                aim,
                deltaTime);
            SampleButton(
                CombatButton.Special,
                _specialKey,
                ref _specialHeldSeconds,
                ref _specialHoldConsumed,
                aim,
                deltaTime);
        }

        public bool TryConsumeBufferedChord(out InputChord chord)
        {
            ExpireBuffer();
            if (_bufferedChord.HasValue)
            {
                chord = _bufferedChord.Value;
                _bufferedChord = null;
                return true;
            }

            chord = default;
            return false;
        }

        public void ClearBuffer()
        {
            _bufferedChord = null;
        }

        private void SampleButton(
            CombatButton button,
            KeyCode key,
            ref float heldSeconds,
            ref bool holdConsumed,
            AimDirection aim,
            float deltaTime)
        {
            if (Input.GetKey(key))
            {
                heldSeconds += deltaTime;
                if (!holdConsumed && heldSeconds >= _holdThresholdSeconds)
                {
                    Buffer(new InputChord(button, ChargeKind.Hold, aim));
                    holdConsumed = true;
                }

                return;
            }

            if (!holdConsumed && heldSeconds > 0f)
            {
                Buffer(new InputChord(button, ChargeKind.Tap, aim));
            }

            heldSeconds = 0f;
            holdConsumed = false;
        }

        private void Buffer(InputChord chord)
        {
            _bufferedChord = chord;
            _bufferExpireTime = Time.time + _bufferSeconds;
        }

        private void ExpireBuffer()
        {
            if (_bufferedChord.HasValue && Time.time > _bufferExpireTime)
            {
                _bufferedChord = null;
            }
        }
    }
}
