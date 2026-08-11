using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private readonly MyCharacterController _controller;
        private float _groundedGravity = -0.6f;
        
        public GroundedState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            MyCharacterController controller) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
            _controller = controller;

            _inputReader.onJumpPerformed += OnJump;
        }

        public override int Key()
        {
            return States.Grounded;
        }

        protected override void OnEnterState()
        {
            Debug.Log("enter ground");
            _velocity.SetVelocity(Vector3.up * _groundedGravity);
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
            
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
        }

        private void OnJump()
        {
            SwitchState(_factory.Get(States.Jump));
        }
    }
}