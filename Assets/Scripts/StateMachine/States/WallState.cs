using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallState : BaseState, IRootState
    {
        private readonly InputReader _inputReader;
        private readonly UnitVelocity _velocity;
        private readonly Transform _model;
        private readonly Transform _player;
        private readonly PlayerPivot _playerPivot;
        private readonly int _layerMask;

        public WallState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            InputReader inputReader,
            UnitVelocity velocity,
            Transform model,
            Transform player,
            PlayerPivot playerPivot,
            int layerMask) : base(currentContext, unitStateFactory)
        {
            _inputReader = inputReader;
            _velocity = velocity;
            _model = model;
            _player = player;
            _playerPivot = playerPivot;
            _layerMask = layerMask;
        }

        public override int Key()
        {
            return States.Wall;
        }

        protected override void OnEnterState()
        {
            _playerPivot.SetPivotValues(_player.position);
            _playerPivot.enabled = true;
            _velocity.SetVelocity(Vector3.zero);
            WallDetection();
            _inputReader.onCrouchActivated += ReleaseWall;
            SetSubState(_factory.Get(States.WallIdle));
        }

        protected override void OnUpdateState()
        {
        }

        protected override void ExitState()
        {
            _playerPivot.enabled = false;
            _inputReader.onCrouchActivated -= ReleaseWall;
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
            
            _inputReader.onCrouchActivated -= ReleaseWall;
        }
        
        private void WallDetection()
        {
            Vector3 upDirection = _model.up;

            if (!Physics.SphereCast(
                    _model.position + upDirection,
                    radius: 0.2f,
                    _model.forward,
                    out RaycastHit hit,
                    1f,
                    _layerMask))
            {
                return;
            }

            _model.forward = -hit.normal;

            Quaternion targetRotation =
                Quaternion.FromToRotation(
                    _player.up,
                    hit.normal) *
                _player.rotation;

            _player.position = hit.point;
            _player.rotation = targetRotation;
        }
        
        private void ReleaseWall()
        {
            Vector3 releasePosition =
                _player.position + _player.up * 0.5f;

            _player.position = releasePosition;
            _player.rotation = Quaternion.identity;
            
            SwitchState(_factory.Get(States.Air));
        }
    }
}