using Synty.AnimationBaseLocomotion.Samples.InputSystem;

namespace UnitStateMachine.PlayerStates
{
    public class WallIdleState : BaseState
    {
        private readonly InputReader _inputReader;

        public WallIdleState(
            StateMachine currentContext, 
            StateFactory unitStateFactory, 
            InputReader inputReader) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
        }

        public override int Key()
        {
            return States.WallIdle;
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
            if (_inputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.WallMove));
            }
        }

        protected override void InitializeSubState()
        {
        }
    }
}