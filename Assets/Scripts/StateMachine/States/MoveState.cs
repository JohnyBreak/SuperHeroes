using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class MoveState : BaseState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private Transform _cameraTransform;
        private readonly Transform _model;
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 6f;
        private Animator _animator;
        private readonly PlayerSharedData _sharedData;

        public MoveState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            Transform cameraTransform,
            Transform model,
            Animator animator,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
            _cameraTransform = cameraTransform;
            _model = model;
            _animator = animator;
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Move;
        }

        protected override void OnEnterState()
        {
        }

        protected override void OnUpdateState()
        {
            bool hasInput =
                _inputReader._moveComposite.sqrMagnitude > 0.0001f;
            Vector3 desiredVelocity = Vector3.zero;
            
            if (hasInput)
            {
                Vector3 input = new Vector3(
                    _inputReader._moveComposite.x,
                    0f,
                    _inputReader._moveComposite.y);
                Quaternion cameraRotation = Quaternion.Euler(
                    0f,
                    _cameraTransform.eulerAngles.y,
                    0f);
                
                Vector3 movementDirection =
                    (cameraRotation * input).normalized;
                
                desiredVelocity = movementDirection * GetSpeed();
                
                RotateModel(
                    movementDirection,
                    Time.deltaTime);
            }
            
            _animator?.SetFloat(
                _sharedData.MovementHash,
                GetAnimationSpeed(),
                0.75f,
                Time.deltaTime);
            
            _velocity.SetXZVelocity(desiredVelocity);
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (!_inputReader._movementInputDetected)
            {
                SwitchState(_factory.Get(States.Idle));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        private void RotateModel(
            Vector3 movementDirection,
            float deltaTime)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(
                movementDirection,
                Vector3.up);
            
            _model.rotation = Quaternion.Slerp(
                _model.rotation,
                desiredRotation,
                _rotationSpeed * deltaTime);
        }
    
        private float GetAnimationSpeed() 
        {
            return (_inputReader._movementInputDetected) ? GetSpeed() : 0;
        }

        private float GetSpeed() 
        {
            return (_inputReader.SprintDetected) ? _moveSpeed * 2 : _moveSpeed;
        }
    }
}