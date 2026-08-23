using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class JumpState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private bool _groundSnapToggleOnce;
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 0.05f;
        //private Vector3 _initialZXVelocity;
        
        public JumpState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Jump;
        }

        protected override void OnEnterState()
        {
            
            _sharedData.Animator?.CrossFadeInFixedTime(_sharedData.JumpHash, 0.1f);
            
            _sharedData.Controller.ShouldSnapToGround = false;
            _sharedData.PreviousYVelocity = _sharedData.InitialJumpVelocity;
            _sharedData.Velocity.SetYVelocity(_sharedData.InitialJumpVelocity);
            _groundSnapToggleOnce = false;
            _sharedData.InputReader.onWallCheckPerformed += WallDetection;
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
            
            _sharedData.Velocity.AddVelocity(desiredVelocity);

            if (_sharedData.Velocity.GetVelocity().y <= 0)
            {
                if (_sharedData.InputReader.WebDetected)
                {
                    SwitchState(_factory.Get(States.Swing));
                    return;
                }
                
                if (!_groundSnapToggleOnce)
                {
                    _sharedData.Controller.ShouldSnapToGround = true;
                    _groundSnapToggleOnce = true;
                
                    _sharedData.Animator?.CrossFadeInFixedTime(_sharedData.FallHash, 0.1f);
                }
            }
            
            HandleGravity();
        }
        
        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.JumpGravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.MaxFallGravity);
            
            _sharedData.Velocity.SetYVelocity(appliedY);
         }
        
        protected override void ExitState()
        {
            _groundSnapToggleOnce = false;
            _sharedData.Controller.ShouldSnapToGround = true;
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }

        protected override void CheckSwitchState()
        {
            // if current Y velocity newar 0 switch to fall
            if (_sharedData.Velocity.GetVelocity().y <= 0 && _sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }

        protected override void InitializeSubState()
        {
        }
        
        protected override void OnDispose()
        {
            base.OnDispose();
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }
        
        private void WallDetection()
        {
            Vector3 upDirection = _sharedData.ModelT.up;

            if (!Physics.SphereCast( 
                    _sharedData.ModelT.position + upDirection,
                    radius: 0.2f,
                    _sharedData.ModelT.forward,
                    out RaycastHit hit,
                    1f,
                    _sharedData.WallMask))
            {
                return;
            }
            
            SwitchState(_factory.Get(States.Wall));
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
            return GetSpeed();
        }

        private float GetSpeed() 
        {
            return _moveSpeed;
        }
    }
}