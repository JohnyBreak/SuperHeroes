using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class FallIdleState: BaseState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        
        public FallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory, 
            InputReader inputReader,
            UnitVelocity velocity) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
        }

        public override int Key()
        {
            return States.FallIdle;
        }

        protected override void OnEnterState()
        {
            Debug.Log("enter idle");
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
                SwitchState(_factory.Get(States.FallMove));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}