using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;
using UnitStateMachine;
using UnitStateMachine.PlayerStates;

public class Player : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private MyCharacterController _controller;
    [SerializeField] private Transform _model;
    
    private StateMachine _stateMachine;
    private StateFactory _stateFactory;
    
    private UnitVelocity _velocity = new UnitVelocity();
    private void Start()
    {
        _stateMachine = new StateMachine();
        _stateFactory = new StateFactory();
        
        GroundedState grounded = new GroundedState(
            _stateMachine, 
            _stateFactory, 
            _inputReader);
        
        MoveState move = new MoveState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity, 
            _cameraTransform,
            _model,
            null);
        
        IdleState idle = new IdleState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity);
        
        _stateMachine.SetState(grounded);
        _stateMachine.Start();
    }

    private void Update()
    {
        _stateMachine.Tick();
        _controller.Move(_velocity.GetVelocity());
    }
}
