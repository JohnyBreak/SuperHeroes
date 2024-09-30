using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _moveSpeed = 6f;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Animator _animator;

    private int _movementHash = Animator.StringToHash("MovementSpeed");

    void Update()
    {
        if (_inputReader._movementInputDetected) 
        {
            Vector3 cameraEuler = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0) * new Vector3(_inputReader._moveComposite.x, 0, _inputReader._moveComposite.y);
            Vector3 movementDirection = cameraEuler.normalized;

            Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);

            _characterController.Move(movementDirection * GetSpeed() * Time.deltaTime);
        }

        _animator.SetFloat(_movementHash, GetAnimationSpeed(), 0.75f, 0.2f);
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
