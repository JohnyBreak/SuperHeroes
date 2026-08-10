using System.Collections.Generic;

namespace UnitStateMachine
{
    public static class States
    {
        public const int Idle = 0;
        public const int Move = 1;
        public const int Grounded = 2;
        public const int Air = 3;
    }

    public class StateFactory
    {
        private Dictionary<int, BaseState> _states = new Dictionary<int, BaseState>();

        public void AddState(BaseState state)
        {
            _states[state.Key()] = state;
        }

        public BaseState Get(int key)
        {
            return _states.GetValueOrDefault(key);
        }
    }
}

