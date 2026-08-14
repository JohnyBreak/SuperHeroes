using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallMoveState : BaseState
    {
        private readonly PlayerSharedData _sharedData;
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 6f;
        private float _surfaceOffset = 0.05f;
        
        public WallMoveState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.WallMove;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
            StickToSurface();
            AlignToSurface();

            Vector3 velocity = Vector3.zero;

            if (_sharedData.InputReader._moveComposite.sqrMagnitude > 0.0001f)
            {
                Vector3 moveDirection =
                    CalculateMoveDirection();

                Quaternion moveRotation =
                    Quaternion.LookRotation(
                        moveDirection,
                        _sharedData.PlayerT.up);

                _sharedData.ModelT.rotation = Quaternion.Lerp(
                    _sharedData.ModelT.rotation,
                    moveRotation,
                    _rotationSpeed * Time.deltaTime);

                velocity = moveDirection * GetSpeed();
            }

            _sharedData.Velocity.SetVelocity(velocity);
            
            _sharedData.Animator?.SetFloat(
                _sharedData.MovementHash,
                GetAnimationSpeed(),
                0.75f,
                Time.deltaTime);
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (!_sharedData.InputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.WallIdle));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        private void StickToSurface()
        {
            Vector3 upDirection = _sharedData.PlayerT.up;
            Vector3 downDirection = -upDirection;

            Vector3 origin =
                _sharedData.PlayerT.position + upDirection * 0.5f;

            if (!Physics.Raycast(
                    origin,
                    downDirection,
                    out RaycastHit hit,
                    1f,
                    _sharedData.WallMoveMask))
            {
                return;
            }

            _sharedData.PlayerT.position =
                hit.point + upDirection * _surfaceOffset;
        }

        private void AlignToSurface()
        {
            Quaternion desiredRotation =
                Quaternion.FromToRotation(
                    _sharedData.PlayerT.up,
                    _sharedData.Pivot.Pivot.up) *
                _sharedData.PlayerT.rotation;

            _sharedData.PlayerT.rotation = Quaternion.Lerp(
                _sharedData.PlayerT.rotation,
                desiredRotation,
                _rotationSpeed * 2f * Time.deltaTime);
        }
        
        private Vector3 CalculateMoveDirection()
        {
            Vector3 forward = Vector3.ProjectOnPlane(
                _sharedData.CameraTransform.forward,
                _sharedData.Pivot.Pivot.up).normalized;

            Vector3 right = Vector3.ProjectOnPlane(
                _sharedData.CameraTransform.right,
                _sharedData.Pivot.Pivot.up).normalized;

            Vector3 direction =
                forward * _sharedData.InputReader._moveComposite.y +
                right * _sharedData.InputReader._moveComposite.x;

            return direction.normalized;
        }
        
        private float GetAnimationSpeed()
        {
            return _sharedData.InputReader._movementInputDetected
                ? GetSpeed()
                : 0f;
        }

        private float GetSpeed()
        {
            return _sharedData.InputReader.SprintDetected
                ? _moveSpeed * 2f
                : _moveSpeed;
        }
    }
}