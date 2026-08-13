using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private readonly MyCharacterController _controller;
        private readonly PlayerSharedData _sharedData;
        private readonly Transform _model;
        private readonly int _layerMask;
        private float _groundedGravity = -0.6f;
        
        public GroundedState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            MyCharacterController controller,
            PlayerSharedData sharedData,
            Transform model,
            int layerMask) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
            _controller = controller;
            _sharedData = sharedData;
            _model = model;
            _layerMask = layerMask;
        }

        public override int Key()
        {
            return States.Grounded;
        }

        protected override void OnEnterState()
        {
            _sharedData.PreviousYVelocity = _groundedGravity;
            _velocity.SetVelocity(Vector3.up * _groundedGravity);
            _inputReader.onJumpPerformed += OnJump;
            _inputReader.onWallCheckPerformed += WallDetection;
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
            _inputReader.onJumpPerformed -= OnJump;
            _inputReader.onWallCheckPerformed -= WallDetection;
        }

        protected override void CheckSwitchState()
        {
            if (!_controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Air));
            }
        }

        protected override void InitializeSubState()
        {
            if (_inputReader._movementInputDetected)
            {
                SetSubState(_factory.Get(States.Move));
                return;
            }
            SetSubState(_factory.Get(States.Idle));
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _inputReader.onJumpPerformed -= OnJump;
            _inputReader.onWallCheckPerformed -= WallDetection;
        }

        private void OnJump()
        {
            SwitchState(_factory.Get(States.Jump));
        }

        private void WallDetection()
        {
            Vector3 upDirection = _model.up;

            if (!Physics.SphereCast( 
                    _model.position + upDirection,
                    radius: 0.2f,
                    _model.forward,
                    out RaycastHit hit,
                    1f,
                    _layerMask))
            {
                return;
            }

            SwitchState(_factory.Get(States.Wall));
            
            //StartChange(
            //    CharacterLocomotionMode.Wall,
            //    hit.point,
            //    targetRotation);
        }
    }
}