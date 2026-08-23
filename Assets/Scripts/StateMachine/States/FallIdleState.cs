using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class FallIdleState: BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public FallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.FallIdle;
        }

        protected override void OnEnterState()
        {
            Debug.Log("enter idle");
            //_sharedData.Velocity.ZeroXZVelocity();
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (_sharedData.InputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.FallMove));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}