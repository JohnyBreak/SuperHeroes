using DG.Tweening;
using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class WallToGroundTransitionState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private readonly string _dotweenID = nameof(WallToGroundTransitionState);
        public WallToGroundTransitionState(
            StateMachine currentContext, 
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.WallToGroundTransition;
        }

        protected override void OnEnterState()
        {
            _sharedData.Controller.ShouldSnapToGround = false;
            _sharedData.Velocity.SetVelocity(Vector3.zero);
            
            _sharedData.Animator?.CrossFadeInFixedTime(_sharedData.TPoseHash, 0.1f);
            
            var origin = _sharedData.PlayerT.position + _sharedData.PlayerT.up;
            var direction = Vector3.down;
            
            Debug.DrawRay(origin, direction * 2f, Color.red, 5f);
            
            float duration = 5f;
            Quaternion targetRotation = Quaternion.LookRotation(_sharedData.PlayerT.up, Vector3.up);
            if (Physics.Raycast(origin, direction, out var hit, 2f, _sharedData.GroundMask))
            {
                DOTween.Kill(_dotweenID);
                Sequence seq = DOTween.Sequence(_dotweenID);
                
                seq.Join(_sharedData.PlayerT.DOMove(hit.point, duration));
                seq.Join(_sharedData.PlayerT.DORotateQuaternion(Quaternion.identity, duration));
                seq.Join(_sharedData.ModelT.DORotateQuaternion(targetRotation, duration));
                
                seq.OnComplete(() =>
                {
                    SwitchState(_factory.Get(States.Grounded));
                });
                
                return;
            }

            SwitchState(_factory.Get(States.Grounded));
        }

        protected override void OnUpdateState()
        {
            
        }

        protected override void ExitState()
        {
            
        }

        protected override void CheckSwitchState()
        {
            
        }

        protected override void InitializeSubState()
        {
            
        }
    }
}