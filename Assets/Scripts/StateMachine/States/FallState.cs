using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class FallState : BaseState, IRootState
    {
        private readonly MyCharacterController _controller;
        private readonly PlayerSharedData _sharedData;
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        
        public FallState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            UnitVelocity velocity,
            InputReader inputReader,
            MyCharacterController controller,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _velocity = velocity;
            _inputReader = inputReader;
            _controller = controller;
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Air;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
            HandleGravity();
        }

        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.Gravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.MaxFallGravity);
            
            //_velocity.AddVelocity(Vector3.up * appliedY);
            _velocity.SetYVelocity(appliedY);
            return;
            var current = _velocity.GetVelocity().y;
            
            if (current <= _sharedData.MaxFallGravity)
            {
                return;
            }
            
            var additionalGravity = _sharedData.Gravity * Time.deltaTime;
            
            if ((current + additionalGravity) < _sharedData.MaxFallGravity)
            {
                additionalGravity = _sharedData.MaxFallGravity - current;
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