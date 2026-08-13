using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallMoveState : BaseState
    {
        private readonly InputReader _inputReader;
        private readonly Transform _playerPivot;
        private readonly Transform _cameraTransform;
        private readonly Transform _playerT;
        private readonly Transform _model;
        private readonly PlayerSharedData _sharedData;
        private readonly Animator _animator;
        private readonly UnitVelocity _velocity;
        private readonly int _layerMask;
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 6f;
        private float _surfaceOffset = 0.05f;
        
        public WallMoveState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader,
            Transform playerPivot,
            Transform cameraTransform,
            Transform playerT,
            Transform model,
            PlayerSharedData sharedData,
            Animator animator,
            UnitVelocity velocity,
            int layerMask) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _playerPivot = playerPivot;
            _cameraTransform = cameraTransform;
            _playerT = playerT;
            _model = model;
            _sharedData = sharedData;
            _animator = animator;
            _velocity = velocity;
            _layerMask = layerMask;
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

            if (_inputReader._moveComposite.sqrMagnitude > 0.0001f)
            {
                Vector3 moveDirection =
                    CalculateMoveDirection();

                Quaternion moveRotation =
                    Quaternion.LookRotation(
                        moveDirection,
                        _playerT.up);

                _model.rotation = Quaternion.Lerp(
                    _model.rotation,
                    moveRotation,
                    _rotationSpeed * Time.deltaTime);

                velocity = moveDirection * GetSpeed();
            }

            _velocity.SetVelocity(velocity);
            
            _animator?.SetFloat(
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
            if (!_inputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.WallIdle));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        private void StickToSurface()
        {
            Vector3 upDirection = _playerT.up;
            Vector3 downDirection = -upDirection;

            Vector3 origin =
                _playerT.position + upDirection * 0.5f;

            if (!Physics.Raycast(
                    origin,
                    downDirection,
                    out RaycastHit hit,
                    1f,
                    _layerMask))
            {
                return;
            }

            _playerT.position =
                hit.point + upDirection * _surfaceOffset;
        }

        private void AlignToSurface()
        {
            Quaternion desiredRotation =
                Quaternion.FromToRotation(
                    _playerT.up,
                    _playerPivot.up) *
                _playerT.rotation;

            _playerT.rotation = Quaternion.Lerp(
                _playerT.rotation,
                desiredRotation,
                _rotationSpeed * 2f * Time.deltaTime);
        }
        
        private Vector3 CalculateMoveDirection()
        {
            Vector3 forward = Vector3.ProjectOnPlane(
                _cameraTransform.forward,
                _playerPivot.up).normalized;

            Vector3 right = Vector3.ProjectOnPlane(
                _cameraTransform.right,
                _playerPivot.up).normalized;

            Vector3 direction =
                forward * _inputReader._moveComposite.y +
                right * _inputReader._moveComposite.x;

            return direction.normalized;
        }
        
        private float GetAnimationSpeed()
        {
            return _inputReader._movementInputDetected
                ? GetSpeed()
                : 0f;
        }

        private float GetSpeed()
        {
            return _inputReader.SprintDetected
                ? _moveSpeed * 2f
                : _moveSpeed;
        }
    }
}