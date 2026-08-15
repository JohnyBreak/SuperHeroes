namespace UnitStateMachine.PlayerStates
{
    public class SwingState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;

        public SwingState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Swing;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
        }

        protected override void InitializeSubState()
        {
        }
    }
}