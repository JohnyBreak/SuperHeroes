using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private readonly PlayerSharedData _sharedData;

        public IdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory, 
            InputReader inputReader,
            UnitVelocity velocity,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Idle;
        }

        protected override void OnEnterState()
        {
            _velocity.ZeroXZVelocity();
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (_inputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.Move));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}