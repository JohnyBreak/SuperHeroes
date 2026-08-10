using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class GroundedState : BaseState, IRootState
    {
        private readonly InputReader _inputReader;

        public GroundedState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            InputReader inputReader) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
        }

        public override int Key()
        {
            return States.Grounded;
        }

        public override void EnterState()
        {
            Debug.Log("enter ground");
            InitializeSubState();
        }

        public override void OnUpdateState()
        {
            
        }

        public override void ExitState()
        {
            
        }

        public override void CheckSwitchState()
        {
            
        }

        public override void InitializeSubState()
        {
            if (_inputReader._movementInputDetected)
            {
                SetSubState(_factory.Get(States.Move));
                return;
            }
            SetSubState(_factory.Get(States.Idle));
        }
    }
}