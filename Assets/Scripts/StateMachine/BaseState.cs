using System;

namespace UnitStateMachine
{
    public abstract class BaseState : IDisposable
    {
        private StateMachine _ctx;
        protected StateFactory _factory;
        private BaseState _currentSuperState;
        private BaseState _currentSubState;

        public abstract int Key();

        protected abstract void OnEnterState();
        
        protected abstract void OnUpdateState();
        protected abstract void ExitState();
        protected abstract void CheckSwitchState();
        protected abstract void InitializeSubState();
        protected virtual void OnDispose() { }
        
        protected BaseState(StateMachine currentContext, StateFactory unitStateFactory)
        {
            _ctx = currentContext;
            _factory = unitStateFactory;
            _factory.AddState(this);
        }
        
        public void EnterState()
        {
            OnEnterState();
            InitializeSubState();
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
        
        public void ExitStates()
        {
            ExitState();
            if (_currentSubState != null)
            {
                _currentSubState.ExitStates();
            }
        }
        
        protected void SwitchState(BaseState newState)
        {
            ExitStates();

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

        public void Dispose()
        {
            OnDispose();
            _currentSubState?.Dispose();
        }
    }
}