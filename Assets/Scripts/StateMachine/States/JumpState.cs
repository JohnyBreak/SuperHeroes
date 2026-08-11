using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class JumpState : BaseState, IRootState
    {
        private readonly MyCharacterController _controller;
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private const float _gravity = -9.8f;
        private const float _maxFallGravity = -35f;

        public JumpState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            MyCharacterController controller) : base(currentContext, unitStateFactory)
        {
            _controller = controller;
            _inputReader = inputReader;
            _velocity = velocity;
        }

        public override int Key()
        {
            return States.Jump;
        }

        protected override void OnEnterState()
        {
            Debug.Log(_velocity.GetVelocity());
            _velocity.AddVelocity(Vector3.up * 35);
            // apply jump force
        }

        protected override void OnUpdateState()
        {
            // apply gravity
            HandleGravity();
        }
        
        private void HandleGravity()
        {
            var current = _velocity.GetVelocity().y;
            
            if (current <= _maxFallGravity)
            {
                return;
            }
            
            var additionalGravity = _gravity * Time.deltaTime;
            
            if ((current + additionalGravity) < _maxFallGravity)
            {
                additionalGravity = _maxFallGravity - current;
            }
            
            _velocity.AddVelocity(Vector3.up * additionalGravity);
        }
        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            // if current Y velocity newar 0 switch to fall
            if (_controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}