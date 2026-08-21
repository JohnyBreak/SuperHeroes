using System;
using System.Collections.Generic;

namespace UnitStateMachine
{
    public static class States
    {
        public const int Idle = 0;
        public const int Move = 1;
        public const int Grounded = 2;
        public const int Air = 3;
        public const int FallIdle = 4;
        public const int FallMove = 5;
        public const int Jump = 6;
        public const int Wall = 7;
        public const int WallIdle = 8;
        public const int WallMove = 9;
        public const int Swing = 10;
        public const int WallToGroundTransition = 11;
    }

    public class StateFactory : IDisposable
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

        public void Dispose()
        {
            foreach (var keyValuePair in _states)
            {
                keyValuePair.Value?.Dispose();
            }
        }
    }
}

