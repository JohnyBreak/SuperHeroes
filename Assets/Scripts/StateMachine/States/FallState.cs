using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class FallState : BaseState, IRootState
    {
        private readonly MyCharacterController _controller;
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private const float _gravity = -9.8f;
        private const float _maxFallGravity = -35f;
        
        public FallState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            UnitVelocity velocity,
            InputReader inputReader,
            MyCharacterController controller) : base(currentContext, unitStateFactory)
        {
            _velocity = velocity;
            _inputReader = inputReader;
            _controller = controller;
        }

        public override int Key()
        {
            return States.Air;
        }

        protected override void OnEnterState()
        {
            //_velocity.SetVelocity(Vector3.up * (_gravity * Time.deltaTime));
        }

        protected override void OnUpdateState()
        {
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
            if (_controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
            if (_inputReader._movementInputDetected)
            {
                SetSubState(_factory.Get(States.FallMove));
                return;
            }
            SetSubState(_factory.Get(States.FallIdle));
        }
    }
}