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

    [SerializeField] private Transform _model;
    //[SerializeField] private Transform _playerPivot;

    private int _crouchtHash = Animator.StringToHash("IsCrouching");
    private int _movementHash = Animator.StringToHash("MovementSpeed");

    private bool _forwardwall;
    private bool _chest;
    private bool _heap;
    private bool _legs;

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
        var downDir = -transform.up;
        var forwardDir = _model.TransformDirection(new Vector3(0, 0, 1));


        //Debug.DrawRay(transform.position + downDir * 0.8f + forwardDir * 0.6f, forwardDir * 0.6f, Color.green);
        //_forwardwall = Physics.Raycast(transform.position + forwardDir * 0.6f, forwardDir * 0.6f, 1, _mask);
        //Debug.DrawRay(transform.position + downDir * 0.5f + forwardDir * 0.6f, downDir, Color.green);
        //_chest = Physics.Raycast(transform.position + forwardDir * 0.6f, downDir, 1, _mask);
        //Debug.DrawRay(transform.position + downDir * 0.5f, downDir, Color.green);
        //_heap = Physics.Raycast(transform.position, downDir, 1, _mask);
        //Debug.DrawRay(transform.position + downDir * 0.5f + -forwardDir * 0.3f, downDir, Color.green);
        //_legs = Physics.Raycast(transform.position + -forwardDir * 0.3f, downDir, 1, _mask);

        var upDir = transform.TransformDirection(new Vector3(0, 1f, 0));

        Debug.DrawRay(transform.position, -upDir, Color.red);
        Physics.Raycast(transform.position, -upDir, out var hit, 1, _mask);

         transform.position = hit.point + transform.TransformDirection(new Vector3(0, 0.05f, 0));

        var forward = _cameraTransform.forward.normalized;
        var right = _cameraTransform.right.normalized;

        forward = Project(forward.normalized, hit.normal).normalized;
        right = right.normalized;

        //Debug.DrawRay(transform.position, forward, Color.blue);
        //Debug.DrawRay(transform.position, right, Color.red);


        var camForward = _inputReader._moveComposite.y * forward;
        var camRight = _inputReader._moveComposite.x * right;


        var camRelativeDirection = camForward + camRight;

        Debug.DrawRay(transform.position, camRelativeDirection, Color.black);

        Quaternion desiredRotation = Quaternion.FromToRotation(transform.up, hit.normal/* _playerPivot.up;*/) * transform.localRotation;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, desiredRotation, _rotationSpeed * 2 * Time.deltaTime);

        if (_inputReader._moveComposite.sqrMagnitude > 0)
        {
            Quaternion moveRotation = Quaternion.LookRotation(camRelativeDirection, transform.up);

            _model.rotation = Quaternion.Lerp(_model.rotation, moveRotation, _rotationSpeed * Time.deltaTime);

            _characterController.Move(camRelativeDirection * GetSpeed());
        }

        _animator.SetFloat(_movementHash, GetAnimationSpeed(), 0.75f, 0.2f);
    }

    private Vector3 Project(Vector3 forward, Vector3 normal)
    {
        return forward - Vector3.Dot(forward, normal) * normal;
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