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
    private PlayerSharedData _sharedData = new PlayerSharedData();
    
    private void Start()
    {
        _stateMachine = new StateMachine();
        _stateFactory = new StateFactory();
        _sharedData.Init();
        
        GroundedState grounded = new GroundedState(
            _stateMachine, 
            _stateFactory, 
            _inputReader,
            _velocity,
            _controller,
            _sharedData);
        
        MoveState move = new MoveState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity, 
            _cameraTransform,
            _model,
            null,
            _sharedData);
        
        IdleState idle = new IdleState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity,
            _sharedData);
        
        FallState fall = new FallState(
            _stateMachine,
            _stateFactory,
            _velocity,
            _inputReader,
            _controller,
            _sharedData);

        FallMoveState fallMove = new FallMoveState(
            _stateMachine,
            _stateFactory,
            _inputReader,
            _velocity,
            _cameraTransform,
            _model,
            null);
        
        FallIdleState fallIdle = new FallIdleState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity);

        JumpState jump = new JumpState(
            _stateMachine, 
            _stateFactory, 
            _inputReader, 
            _velocity,
            _controller,
            _sharedData);
        
        _stateMachine.SetState(grounded);
        _stateMachine.Start();
    }

    private void Update()
    {
        _stateMachine.Tick();
        _controller.Move(_velocity.GetVelocity());
    }

    private void OnDestroy()
    {
        _stateFactory.Dispose();
    }
}
