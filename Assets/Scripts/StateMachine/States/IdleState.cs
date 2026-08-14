namespace UnitStateMachine.PlayerStates
{
    public class IdleState : BaseState
    {
        private readonly PlayerSharedData _sharedData;

        public IdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Idle;
        }

        protected override void OnEnterState()
        {
            _sharedData.Velocity.ZeroXZVelocity();
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
                SwitchState(_factory.Get(States.Move));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}