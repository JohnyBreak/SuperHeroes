using UnityEngine;

namespace UnitStateMachine.PlayerStates
{
    public class SwingState : BaseState, IRootState
    {
        private readonly PlayerSharedData _sharedData;
        private float _maxDistance = 10;
        private float _steeringSpeed = 2;
        private Vector3 _previousPosition;
        private bool _connected;

        public SwingState(
            StateMachine currentContext,
            StateFactory unitStateFactory,
            PlayerSharedData sharedData) : base(currentContext, unitStateFactory)
        {
            _sharedData = sharedData;
        }

        public override int Key()
        {
            return States.Swing;
        }

        protected override void OnEnterState()
        {
            Vector3 currentPosition = _sharedData.PlayerT.position;
            _previousPosition = currentPosition -
                                _sharedData.Velocity.GetVelocity() * Time.deltaTime;
            
            _connected = true;
            var camForward = _sharedData.CameraTransform.forward;
            camForward.y = 0;
            _sharedData.LineRenderer.enabled = true;
            _sharedData.SwingRoot.position = _sharedData.PlayerT.position +
                                             _sharedData.PlayerT.up * 8 +
                                             camForward * 5;
        }

        protected override void OnUpdateState()
        {
            _connected = _sharedData.InputReader.WebDetected;

            HandleMovement();
            //Rotate();
            UpdateLine();
        }

        protected override void ExitState()
        {
            _connected = false;
            _sharedData.LineRenderer.enabled = false;
        }

        protected override void CheckSwitchState()
        {
            if (_connected)
            {
                return;
            }

            if (_sharedData.Controller.IsGrounded)
            {
                SwitchState(_factory.Get(States.Grounded));
                return;
            }

            SwitchState(_factory.Get(States.Air));
        }

        protected override void InitializeSubState()
        {
        }

        private void Rotate()
        {
            _sharedData.ModelT.up = (_sharedData.SwingRoot.position - _sharedData.PlayerT.position).normalized;
        }

        private void HandleMovement()
        {
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

            Vector3 inputAcceleration = (_sharedData.CameraTransform.forward * (input.z * _steeringSpeed)) +
                                        (_sharedData.CameraTransform.right * (input.x * _steeringSpeed));

            Vector3 currentPosition = _sharedData.PlayerT.position;

            Vector3 acceleration = (Vector3.up * _sharedData.Gravity) + inputAcceleration; // add XZ steering

            Vector3 predictedPosition = currentPosition +
                                        (currentPosition - _previousPosition) +
                                        acceleration * (Time.deltaTime * Time.deltaTime);
            
            if (_connected)
            {
                predictedPosition = ConstrainToRope(predictedPosition);
            }
            
            Vector3 displacement = predictedPosition - currentPosition;
            _previousPosition = currentPosition;
            _sharedData.Velocity.SetVelocity(displacement / Time.deltaTime);
        }

        private void UpdateLine()
        {
            _sharedData.LineRenderer.positionCount = 2;
            _sharedData.LineRenderer.SetPosition(0, _sharedData.SwingRoot.position);
            _sharedData.LineRenderer.SetPosition(1, _sharedData.PlayerT.position);
        }

        private Vector3 ConstrainToRope(Vector3 predictedPosition)
        {
            Vector3 pivot = _sharedData.SwingRoot.position;
            Vector3 toCharacter = predictedPosition - pivot;
            float distance = toCharacter.magnitude;
            if (distance <= _maxDistance || distance < 0.0001f)
            {
                return predictedPosition;
            }
            return pivot + toCharacter / distance * _maxDistance;
        }
    }
}