using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class FallState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        
        public FallState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Air;
        }

        protected override void OnEnterState()
        {
            _sharedData.Animator?.CrossFadeInFixedTime(_sharedData.FallHash, 0.1f);
            _sharedData.InputReader.onWallCheckPerformed += WallDetection;
        }

        protected override void OnUpdateState()
        {
            HandleGravity();
        }

        private void HandleGravity()
        {
            float previousYVelocity = _sharedData.PreviousYVelocity;

            _sharedData.PreviousYVelocity += _sharedData.Gravity * Time.deltaTime;
            float appliedY = Mathf.Max((previousYVelocity + _sharedData.PreviousYVelocity) * .5f,
                _sharedData.MaxFallGravity);
            
            _sharedData.Velocity.SetYVelocity(appliedY);
        }

        protected override void ExitState()
        {
            _sharedData.InputReader.onWallCheckPerformed -= WallDetection;
        }

        protected override void CheckSwitchState()
        {
            if (_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
            }
        }
        
        protected override void InitializeSubState()
        {
            if (_sharedData.InputReader._movementInputDetected)
            {
                SetSubState(_factory.Get(States.FallMove));
                return;
            }
            SetSubState(_factory.Get(States.FallIdle));
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
    }
}