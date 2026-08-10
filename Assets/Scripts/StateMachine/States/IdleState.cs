using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        public IdleState(
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
            return States.Idle;
        }

        public override void EnterState()
        {
            Debug.Log("enter idle");
            _velocity.ZeroXZVelocity();
        }

        public override void OnUpdateState()
        {
        }

        public override void ExitState()
        {
        }

        public override void CheckSwitchState()
        {
            if (_inputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.Move));
            }
        }

        public override void InitializeSubState()
        {
        }
    }
}