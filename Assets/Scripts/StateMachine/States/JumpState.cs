using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class JumpState : BaseState, IRootState
    {
        private readonly MyCharacterController _controller;
        private readonly PlayerSharedData _sharedData;
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private bool _groundSnapToggleOnce;
        
        public JumpState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            MyCharacterController controller,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _controller = controller;
            _sharedData = sharedData;
            _inputReader = inputReader;
            _velocity = velocity;
        }

        public override int Key()
        {
            return States.Jump;
        }

        protected override void OnEnterState()
        {
            _controller.ShouldSnapToGround = false;
            _sharedData.PreviousYVelocity = _sharedData.InitialJumpVelocity;
            _velocity.SetYVelocity(_sharedData.InitialJumpVelocity);
            _groundSnapToggleOnce = false;
            // apply jump force
        }

        protected override void OnUpdateState()
        {
            if (!_groundSnapToggleOnce && _velocity.GetVelocity().y <= 0)
            {
                _controller.ShouldSnapToGround = true;
                _groundSnapToggleOnce = true;
            }

            // apply gravity
            HandleGravity();
        }
        
        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.JumpGravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.MaxFallGravity);
            
            _velocity.SetYVelocity(appliedY);
            return;
            var current = _velocity.GetVelocity().y;
            
            if (current <= _sharedData.MaxFallGravity)
            {
                return;
            }
            
            var additionalGravity = _sharedData.JumpGravity * Time.deltaTime;
            
            if ((current + additionalGravity) < _sharedData.MaxFallGravity)
            {
                additionalGravity = _sharedData.MaxFallGravity - current;
            }
            
            _velocity.AddVelocity(Vector3.up * additionalGravity);
        }
        
        protected override void ExitState()
        {
            _groundSnapToggleOnce = false;
        }

        protected override void CheckSwitchState()
        {
            // if current Y velocity newar 0 switch to fall
            if ( _velocity.GetVelocity().y <= 0 && _controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}