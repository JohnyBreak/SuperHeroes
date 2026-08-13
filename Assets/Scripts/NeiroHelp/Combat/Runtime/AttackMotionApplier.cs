using UnityEngine;

namespace NeiroHelp.Combat
{
    public class AttackMotionApplier
    {
        private Vector3 _previousLocalDisplacement;
        private bool _hasPrevious;

        public void Reset()
        {
            _previousLocalDisplacement = Vector3.zero;
            _hasPrevious = false;
        }

        public Vector3 EvaluateWorldDelta(
            Transform facingTransform,
            AttackMotionData motion,
            float normalizedTime)
        {
            float forward = motion.ForwardDisplacement.Evaluate(normalizedTime) * motion.ForwardDistance;
            float height = motion.HeightDisplacement.Evaluate(normalizedTime) * motion.HeightDistance;
            Vector3 local = new Vector3(0f, height, forward);

            if (!_hasPrevious)
            {
                _previousLocalDisplacement = local;
                _hasPrevious = true;
                return Vector3.zero;
            }

            Vector3 localDelta = local - _previousLocalDisplacement;
            _previousLocalDisplacement = local;

            Vector3 worldDelta =
                facingTransform.forward * localDelta.z
                + Vector3.up * localDelta.y;

            return worldDelta;
        }
    }
}
