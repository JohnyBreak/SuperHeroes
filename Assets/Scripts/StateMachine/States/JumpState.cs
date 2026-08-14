using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class JumpState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private bool _groundSnapToggleOnce;
        
        public JumpState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Jump;
        }

        protected override void OnEnterState()
        {
            _sharedData.Controller.ShouldSnapToGround = false;
            _sharedData.PreviousYVelocity = _sharedData.InitialJumpVelocity;
            _sharedData.Velocity.SetYVelocity(_sharedData.InitialJumpVelocity);
            _groundSnapToggleOnce = false;
            _sharedData.InputReader.onWallCheckPerformed += WallDetection;
        }

        protected override void OnUpdateState()
        {
            if (!_groundSnapToggleOnce && _sharedData.Velocity.GetVelocity().y <= 0)
            {
                _sharedData.Controller.ShouldSnapToGround = true;
                _groundSnapToggleOnce = true;
            }

            HandleGravity();
        }
        
        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.JumpGravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.MaxFallGravity);
            
            _sharedData.Velocity.SetYVelocity(appliedY);
            return;
            var current = _sharedData.Velocity.GetVelocity().y;
            
            if (current <= _sharedData.MaxFallGravity)
            {
                return;
            }
            
            var additionalGravity = _sharedData.JumpGravity * Time.deltaTime;
            
            if ((current + additionalGravity) < _sharedData.MaxFallGravity)
            {
                additionalGravity = _sharedData.MaxFallGravity - current;
            }
            
            _sharedData.Velocity.AddVelocity(Vector3.up * additionalGravity);
        }
        
        protected override void ExitState()
        {
            _groundSnapToggleOnce = false;
            _sharedData.Controller.ShouldSnapToGround = true;
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }

        protected override void CheckSwitchState()
        {
            // if current Y velocity newar 0 switch to fall
            if (_sharedData.Velocity.GetVelocity().y <= 0 && _sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
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
        }
    }
}