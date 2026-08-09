using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class WallMovement : MonoBehaviour
{
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Transform _model;
    [SerializeField] private Transform _playerPivot;
    [SerializeField] private float _surfaceOffset = 0.05f;

    private readonly int _crouchHash =
        Animator.StringToHash("IsCrouching");

    private readonly int _movementHash =
        Animator.StringToHash("MovementSpeed");

    private void OnEnable()
    {
        _animator.SetBool(_crouchHash, true);
    }

    private void OnDisable()
    {
        _animator.SetBool(_crouchHash, false);
    }

    public Vector3 CalculateVelocity()
    {
        StickToSurface();
        AlignToSurface();

        Vector3 velocity = Vector3.zero;

        if (_inputReader._moveComposite.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDirection =
                CalculateMoveDirection();

            Quaternion moveRotation =
                Quaternion.LookRotation(
                    moveDirection,
                    transform.up);

            _model.rotation = Quaternion.Lerp(
                _model.rotation,
                moveRotation,
                _rotationSpeed * Time.deltaTime);

            velocity = moveDirection * GetSpeed();
        }

        _animator.SetFloat(
            _movementHash,
            GetAnimationSpeed(),
            0.75f,
            Time.deltaTime);

        return velocity;
    }

    private void StickToSurface()
    {
        Vector3 upDirection = transform.up;
        Vector3 downDirection = -upDirection;

        Vector3 origin =
            transform.position + upDirection * 0.5f;

        if (!Physics.Raycast(
                origin,
                downDirection,
                out RaycastHit hit,
                1f,
                _mask))
        {
            return;
        }

        transform.position =
            hit.point + upDirection * _surfaceOffset;
    }

    private void AlignToSurface()
    {
        Quaternion desiredRotation =
            Quaternion.FromToRotation(
                transform.up,
                _playerPivot.up) *
            transform.rotation;

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRotation,
            _rotationSpeed * 2f * Time.deltaTime);
    }

    private Vector3 CalculateMoveDirection()
    {
        Vector3 forward = Vector3.ProjectOnPlane(
            _cameraTransform.forward,
            _playerPivot.up).normalized;

        Vector3 right = Vector3.ProjectOnPlane(
            _cameraTransform.right,
            _playerPivot.up).normalized;

        Vector3 direction =
            forward * _inputReader._moveComposite.y +
            right * _inputReader._moveComposite.x;

        return direction.normalized;
    }

    private float GetAnimationSpeed()
    {
        return _inputReader._movementInputDetected
            ? GetSpeed()
            : 0f;
    }

    private float GetSpeed()
    {
        return _inputReader.SprintDetected
            ? _moveSpeed * 2f
            : _moveSpeed;
    }
}