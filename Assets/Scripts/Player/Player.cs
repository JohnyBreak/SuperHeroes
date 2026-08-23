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
    [SerializeField] private LayerMask _wallMask;
    [SerializeField] private PlayerPivot _playerPivot;
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Animator _animator;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _swingRoot;
    
    private StateMachine _stateMachine;
    private StateFactory _stateFactory;
    
    private UnitVelocity _velocity = new UnitVelocity();
    private PlayerSharedData _sharedData;
    
    private void Start()
    {
        _stateMachine = new StateMachine();
        _stateFactory = new StateFactory();
        _sharedData = new PlayerSharedData(
            _velocity, 
            _controller, 
            _playerPivot, 
            transform,
            _model, 
            _inputReader,
            _animator,
            _cameraTransform,
            _wallMask,
            _groundMask,
            _lineRenderer,
            _swingRoot);

        SwingState swing = new SwingState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);
        
        GroundedState grounded = new GroundedState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);
        
        WallToGroundTransitionState wallToGroundTransition = new WallToGroundTransitionState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);
        
        WallState wall = new WallState(
            _stateMachine, 
            _stateFactory,
            _sharedData);
        
        WallIdleState wallIdle = new WallIdleState(
            _stateMachine, 
            _stateFactory,
            _sharedData);
        
        WallMoveState wallMove = new WallMoveState(
            _stateMachine,
            _stateFactory,
            _sharedData);
        
        MoveState move = new MoveState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);
        
        IdleState idle = new IdleState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);
        
        FallState fall = new FallState(
            _stateMachine,
            _stateFactory,
            _sharedData);

        FallMoveState fallMove = new FallMoveState(
            _stateMachine,
            _stateFactory,
            _sharedData);
        
        FallIdleState fallIdle = new FallIdleState(
            _stateMachine, 
            _stateFactory, 
            _sharedData);

        JumpState jump = new JumpState(
            _stateMachine, 
            _stateFactory,
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
        _stateFactory?.Dispose();
    }
}
