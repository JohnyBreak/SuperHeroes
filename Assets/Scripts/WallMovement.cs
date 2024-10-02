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
        var downDir = -transform.up;//transform.TransformDirection(new Vector3(0, -1, 0));

        Debug.DrawRay(transform.position - downDir * 0.5f, downDir, Color.red);

        Physics.Raycast(transform.position, downDir, out var hit, 1, _mask);

        //transform.up = hit.normal;
        transform.position = hit.point + transform.TransformDirection(new Vector3(0, 0.1f, 0));

        Debug.Log(hit.normal);
        Debug.DrawRay(hit.point, hit.normal, Color.yellow);
        Vector3 wallPlane = (transform.forward * _inputReader._moveComposite.y + transform.right * _inputReader._moveComposite.x);
        //Vector3 wallPlane = (transform.TransformDirection(Vector3.forward) * _inputReader._moveComposite.y + transform.TransformDirection(Vector3.right) * _inputReader._moveComposite.x);
        wallPlane.Normalize();

        Vector3 cameraPlane = (wallPlane.x * _cameraTransform.right + wallPlane.y * _cameraTransform.up + wallPlane.z * _cameraTransform.forward);
        cameraPlane.Normalize();

        var upDir = transform.TransformDirection(new Vector3(0, 1, 0));

        //Vector3 testDirection = Quaternion.FromToRotation(_cameraTransform.up, Vector3.up) *
         //   _cameraTransform.TransformDirection(new Vector3(_inputReader._moveComposite.x, 0, _inputReader._moveComposite.y));

        Debug.DrawRay(transform.position + upDir * 0.5f, wallPlane, Color.blue);

        var forward = transform.TransformDirection(new Vector3(0, 0, 1));

        if (_inputReader._movementInputDetected)
        {
            //Quaternion lookRotation = Quaternion.LookRotation(forward, hit.normal);
            //transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, _rotationSpeed * Time.deltaTime);

            //transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            Quaternion desiredRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;//Quaternion.LookRotation(wallPlane, transform.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, _rotationSpeed * Time.deltaTime);

            _characterController.Move(wallPlane * GetSpeed());
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
