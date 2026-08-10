namespace UnitStateMachine
{
    public class StateMachine
    {
        private BaseState _currentState;

        public StateMachine()
        {
        }
        
        public void Start()
        {
            _currentState?.EnterState();
        }
        
        public void SetState(BaseState newState)
        {
            _currentState = newState;
        }

        public void Tick()
        {
            _currentState?.UpdateStates();
        }
    }
}