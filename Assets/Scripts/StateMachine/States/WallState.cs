using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private float _surfaceOffset = 0.05f;
        private float _rotationSpeed = 6f;
        private float _moveSpeed = 6f;

        public WallState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Wall;
        }

        protected override void OnEnterState()
        {
            _sharedData.Animator?.CrossFadeInFixedTime(_sharedData.WallCrawlingHash, 0.1f);
            
            _sharedData.Velocity.SetVelocity(Vector3.zero);
            WallDetection();
            _sharedData.InputReader.onCrouchActivated += ReleaseWall;
            SetSubState(_factory.Get(States.WallIdle));
            _sharedData.Controller.ShouldSnapToGround = false;
        }

        protected override void OnUpdateState()
        {
            UpdateAnimation();
            
            if (DetectGround())
            {
                SwitchState(_factory.Get(States.WallToGroundTransition));
                return;
            }

            if (IsFloorNormal())
            {
                ReleaseWall();
            }
            StickToSurface();
            AlignToSurface();
        }
        
        protected override void ExitState()
        {
            _sharedData.Pivot.enabled = false;
            _sharedData.InputReader.onCrouchActivated -= ReleaseWall;
            _sharedData.Controller.ShouldSnapToGround = true;
        }

        protected override void CheckSwitchState()
        {
        }

        protected override void InitializeSubState()
        {
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            
            _sharedData.InputReader.onCrouchActivated -= ReleaseWall;
        }

        private bool DetectGround()
        {
            var origin = _sharedData.PlayerT.position + _sharedData.PlayerT.up;
            var direction = _sharedData.ModelT.forward;
            
            Debug.DrawRay(origin, direction, Color.blueViolet, 0.1f);
            
            return Physics.SphereCast(origin, 0.2f, direction, out var hit, 1f, _sharedData.GroundMask);
        }

        private bool IsFloorNormal()
        {
            float alignment = Vector3.Dot(
                _sharedData.Pivot.Pivot.up,
                Vector3.up);

            return alignment > 0.73f;
        }
        
        private void StickToSurface()
        {
            Vector3 upDirection = _sharedData.PlayerT.up;
            Vector3 downDirection = -upDirection;

            Vector3 origin =
                _sharedData.PlayerT.position + upDirection * 0.5f;

            if (!Physics.Raycast(
                    origin,
                    downDirection,
                    out RaycastHit hit,
                    1f,
                    _sharedData.WallMask))
            {
                return;
            }

            _sharedData.PlayerT.position =
                hit.point + upDirection * _surfaceOffset;
        }

        private void AlignToSurface()
        {
            Quaternion desiredRotation =
                Quaternion.FromToRotation(
                    _sharedData.PlayerT.up,
                    _sharedData.Pivot.Pivot.up) *
                _sharedData.PlayerT.rotation;

            _sharedData.PlayerT.rotation = Quaternion.Lerp(
                _sharedData.PlayerT.rotation,
                desiredRotation,
                _rotationSpeed * 2f * Time.deltaTime);
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

            _sharedData.ModelT.forward = -hit.normal;

            RotateTo(hit.normal);
            
            _sharedData.PlayerT.position = hit.point;
            
            _sharedData.Pivot.SetPivotValues(_sharedData.PlayerT.position);
            _sharedData.Pivot.enabled = true;
        }
        
        private void ReleaseWall()
        {
            RotateTo(Vector3.up);
            
            if (_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
                return;
            }
            
            SwitchState(_factory.Get(States.Air));
        }
        
        private void RotateTo(Vector3 normal)
        {
            Quaternion targetRotation =
                Quaternion.FromToRotation(
                    _sharedData.PlayerT.up,
                    normal) *
                _sharedData.PlayerT.rotation;
            
            _sharedData.PlayerT.rotation = targetRotation;
        }
        
        private void UpdateAnimation()
        {
            _sharedData.Animator?.SetFloat(
                _sharedData.MovementSpeedHash,
                GetAnimationSpeed(),
                0.1f,
                Time.deltaTime);
        }
        
        private float GetAnimationSpeed()
        {
            return _sharedData.InputReader._movementInputDetected
                ? GetSpeed()
                : 0f;
        }
        
        private float GetSpeed()
        {
            return _sharedData.InputReader.SprintDetected
                ? _moveSpeed * 2f
                : _moveSpeed;
        }
    }
}