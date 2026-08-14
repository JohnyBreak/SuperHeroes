using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class MoveState : BaseState
    {
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 6f;
        private readonly PlayerSharedData _sharedData;

        public MoveState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
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
                _sharedData.InputReader._moveComposite.sqrMagnitude > 0.0001f;
            Vector3 desiredVelocity = Vector3.zero;
            
            if (hasInput)
            {
                Vector3 input = new Vector3(
                    _sharedData.InputReader._moveComposite.x,
                    0f,
                    _sharedData.InputReader._moveComposite.y);
                Quaternion cameraRotation = Quaternion.Euler(
                    0f,
                    _sharedData.CameraTransform.eulerAngles.y,
                    0f);
                
                Vector3 movementDirection =
                    (cameraRotation * input).normalized;
                
                desiredVelocity = movementDirection * GetSpeed();
                
                RotateModel(
                    movementDirection,
                    Time.deltaTime);
            }
            
            _sharedData.Animator?.SetFloat(
                _sharedData.MovementHash,
                GetAnimationSpeed(),
                0.75f,
                Time.deltaTime);
            
            _sharedData.Velocity.SetXZVelocity(desiredVelocity);
        }

        protected override void ExitState()
        {
        }

        protected override void CheckSwitchState()
        {
            if (!_sharedData.InputReader._movementInputDetected)
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
            
            _sharedData.ModelT.rotation = Quaternion.Slerp(
                _sharedData.ModelT.rotation,
                desiredRotation,
                _rotationSpeed * deltaTime);
        }
    
        private float GetAnimationSpeed() 
        {
            return (_sharedData.InputReader._movementInputDetected) ? GetSpeed() : 0;
        }

        private float GetSpeed() 
        {
            return (_sharedData.InputReader.SprintDetected) ? _moveSpeed * 2 : _moveSpeed;
        }
    }
}