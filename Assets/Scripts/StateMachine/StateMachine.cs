using System;
using UnityEngine;

namespace UnitStateMachine
{
    [Serializable]
    public class StateMachine
    {
        [SerializeField] private string CurrentStateName;
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

            CurrentStateName = _currentState != null ? _currentState.GetType().Name : "None";
        }
    }
}