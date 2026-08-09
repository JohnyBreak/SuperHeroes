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
    
    public Vector3 CalculateVelocity()
    {
        Vector3 velocity = Vector3.zero;

        if (_inputReader._movementInputDetected)
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

            Quaternion desiredRotation = Quaternion.LookRotation(
                movementDirection,
                transform.up);

            _model.rotation = Quaternion.Slerp(
                _model.rotation,
                desiredRotation,
                _rotationSpeed * Time.deltaTime);

            velocity = movementDirection * GetSpeed();
        }

        _animator.SetFloat(
            _movementHash,
            GetAnimationSpeed(),
            0.75f,
            Time.deltaTime);

        return velocity;
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
