namespace UnitStateMachine
{
    public abstract class BaseState
    {
        protected StateMachine _ctx;
        protected StateFactory _factory;
        protected BaseState _currentSuperState;
        protected BaseState _currentSubState;

        public abstract int Key();

        public abstract void EnterState();
        public abstract void OnUpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchState();
        public abstract void InitializeSubState();

        protected BaseState(StateMachine currentContext, StateFactory unitStateFactory)
        {
            _ctx = currentContext;
            _factory = unitStateFactory;
            _factory.AddState(this);
        }

        public void UpdateStates()
        {
            OnUpdateState();
            if (_currentSubState != null)
            {
                _currentSubState.UpdateStates();
            }
            CheckSwitchState();
        }
    
        protected void SwitchState(BaseState newState)
        {
            ExitState();

            newState.EnterState();
            
            if (this is IRootState)
            {
                _ctx.SetState(newState);
            }
            else if (_currentSuperState != null)
            {
                _currentSuperState.SetSubState(newState);
            }
        }

        private void SetSuperState(BaseState newSuperState)
        {
            _currentSuperState = newSuperState;
        }

        protected void SetSubState(BaseState newSubState)
        {
            _currentSubState = newSubState;
            newSubState.SetSuperState(this);
        }

        public void ExitStates()
        {
            ExitState();
            if (_currentSubState != null)
            {
                _currentSubState.ExitStates();
            }
        }
    }
}