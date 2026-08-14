using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private float _groundedGravity = -0.6f;
        
        public GroundedState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Grounded;
        }

        protected override void OnEnterState()
        {
            _sharedData.PreviousYVelocity = _groundedGravity;
            _sharedData.Velocity.SetVelocity(Vector3.up * _groundedGravity);
            _sharedData.InputReader.onJumpPerformed += OnJump;
            _sharedData.InputReader.onWallCheckPerformed += WallDetection;
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
            _sharedData.InputReader.onJumpPerformed -= OnJump;
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }

        protected override void CheckSwitchState()
        {
            if (!_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Air));
            }
        }

        protected override void InitializeSubState()
        {
            if (_sharedData.InputReader._movementInputDetected)
            {
                SetSubState(_factory.Get(States.Move));
                return;
            }
            SetSubState(_factory.Get(States.Idle));
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            _sharedData.InputReader.onJumpPerformed -= OnJump;
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }

        private void OnJump()
        {
            SwitchState(_factory.Get(States.Jump));
        }

        private void WallDetection()
        {
            Vector3 upDirection = _sharedData.ModelT.up;

            if (!Physics.SphereCast( 
                    _sharedData.ModelT.position + upDirection,
                    radius: 0.2f,
                    _sharedData.ModelT.forward,
                    out RaycastHit hit,
                    1f,
                    _sharedData.WallDetectMask))
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