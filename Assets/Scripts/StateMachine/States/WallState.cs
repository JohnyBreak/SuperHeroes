using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;

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
            _sharedData.Velocity.SetVelocity(Vector3.zero);
            WallDetection();
            _sharedData.InputReader.onCrouchActivated += ReleaseWall;
            SetSubState(_factory.Get(States.WallIdle));
        }

        protected override void OnUpdateState()
        {
            if (IsFloorNormal())
            {
                ReleaseWall();
            }
        }

        protected override void ExitState()
        {
            _sharedData.Pivot.enabled = false;
            _sharedData.InputReader.onCrouchActivated -= ReleaseWall;
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
        
        private bool IsFloorNormal()
        {
            float alignment = Vector3.Dot(
                _sharedData.Pivot.Pivot.up,
                Vector3.up);

            return alignment > 0.85f;
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
                    _sharedData.WallDetectMask))
            {
                return;
            }

            _sharedData.ModelT.forward = -hit.normal;

            Quaternion targetRotation =
                Quaternion.FromToRotation(
                    _sharedData.PlayerT.up,
                    hit.normal) *
                _sharedData.PlayerT.rotation;

            _sharedData.PlayerT.position = hit.point;
            _sharedData.PlayerT.rotation = targetRotation;
            
            _sharedData.Pivot.SetPivotValues(_sharedData.PlayerT.position);
            _sharedData.Pivot.enabled = true;
        }
        
        private void ReleaseWall()
        {
            _sharedData.PlayerT.rotation = Quaternion.identity;

            if (_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
                return;
            }
            
            //Vector3 releasePosition = _player.position + _player.up * 0.5f;
            //_player.position = releasePosition;
            SwitchState(_factory.Get(States.Air));
        }
    }
}