using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class WallMovement : MonoBehaviour
{
    [SerializeField] private MyCharacterController _characterController;

    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private InputReader _inputReader;

    private int _crouchtHash = Animator.StringToHash("IsCrouching");
    private int _movementHash = Animator.StringToHash("MovementSpeed");

    private void OnEnable()
    {
        _animator.SetBool(_crouchtHash, true);
    }

    private void OnDisable()
    {
        _animator.SetBool(_crouchtHash, false);
    }

    void Update()
    {
        var dir = transform.TransformDirection(new Vector3(0, -1, 0));
        Debug.DrawRay(transform.position - dir * 0.5f, dir, Color.red);
        Physics.Raycast(transform.position, dir, out var hit, 1, _mask);

        transform.up = hit.normal;

        Vector3 cameraPlane = (transform.TransformDirection(Vector3.right) * _inputReader._moveComposite.x + transform.TransformDirection(Vector3.forward) * _inputReader._moveComposite.y);
        cameraPlane.Normalize();

        Vector3 cameraEuler = Quaternion.Euler(hit.normal.x, hit.normal.y, hit.normal.z) * new Vector3(_inputReader._moveComposite.x, 0, _inputReader._moveComposite.y);
        Vector3 movementDirection = cameraEuler.normalized;

        Debug.DrawRay(transform.position + hit.normal * 0.5f, cameraPlane, Color.blue);

        Quaternion desiredRotation = Quaternion.LookRotation(movementDirection, hit.normal);

        //transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);
        if (_inputReader._movementInputDetected)
        {
            _characterController.Move(cameraPlane * GetSpeed()/* * Time.deltaTime*/);
        }

        //_animator.SetFloat(_movementHash, GetAnimationSpeed(), 0.75f, 0.2f);
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
