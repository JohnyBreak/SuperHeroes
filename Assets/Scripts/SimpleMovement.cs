using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _moveSpeed = 6f;

    [SerializeField] private MyCharacterController _characterController;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Animator _animator;

    [SerializeField] private Transform _model;
    
    [SerializeField] private float _airControlAcceleration = 4f;
    [SerializeField, Range(0f, 1f)]
    private float _airRotationMultiplier = 0.35f;
    private Vector3 _horizontalVelocity;
    
    private int _movementHash = Animator.StringToHash("MovementSpeed");

    void Update()
    {
        if (_inputReader._movementInputDetected) 
        {
            Vector3 cameraEuler = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0) * new Vector3(_inputReader._moveComposite.x, 0, _inputReader._moveComposite.y);
            Vector3 movementDirection = cameraEuler.normalized;

            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, transform.up);

            _model.rotation = Quaternion.Slerp(_model.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);

            _characterController.Move(movementDirection * GetSpeed());
        }

        _animator.SetFloat(_movementHash, GetAnimationSpeed(), 0.75f, 0.2f);
    }
    
    public Vector3 CalculateVelocity(
        float deltaTime,
        bool useGroundControl)
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
            desiredVelocity =
                movementDirection * GetSpeed();
            RotateModel(
                movementDirection,
                useGroundControl,
                deltaTime);
        }
        if (useGroundControl)
        {
            _horizontalVelocity = desiredVelocity;
        }
        else if (hasInput)
        {
            _horizontalVelocity = Vector3.MoveTowards(
                _horizontalVelocity,
                desiredVelocity,
                _airControlAcceleration * deltaTime);
        }
        _animator.SetFloat(
            _movementHash,
            GetAnimationSpeed(),
            0.75f,
            deltaTime);
        return _horizontalVelocity;
    }
    private void RotateModel(
        Vector3 movementDirection,
        bool useGroundControl,
        float deltaTime)
    {
        Quaternion desiredRotation = Quaternion.LookRotation(
            movementDirection,
            transform.up);
        float rotationMultiplier =
            useGroundControl ? 1f : _airRotationMultiplier;
        _model.rotation = Quaternion.Slerp(
            _model.rotation,
            desiredRotation,
            _rotationSpeed * rotationMultiplier * deltaTime);
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
