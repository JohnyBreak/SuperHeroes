using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallIdleState : BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public WallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory, 
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.WallIdle;
        }

        protected override void OnEnterState()
        {
            _sharedData.Velocity.SetVelocity(Vector3.zero);
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
                SwitchState(_factory.Get(States.WallMove));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}