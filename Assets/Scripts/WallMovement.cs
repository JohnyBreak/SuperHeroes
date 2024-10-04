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
        var downDir = -transform.up;

        Debug.DrawRay(transform.position - downDir * 0.5f, downDir, Color.red);

        Physics.Raycast(transform.position - downDir * 0.5f, downDir, out var hit, 1, _mask);

        transform.position = hit.point + transform.TransformDirection(new Vector3(0, 0.1f, 0));

        var forward = _cameraTransform.forward.normalized;
        var right = _cameraTransform.right.normalized; 

        //var forward = _cameraTransform.InverseTransformVector(_cameraTransform.forward).normalized;
        //var right = _cameraTransform.InverseTransformVector(_cameraTransform.right).normalized;

        var upDir = transform.TransformDirection(new Vector3(0, 1, 0));
        
        //forward.y = 0;
        //right.y = 0;

        forward = Project(forward.normalized, hit.normal).normalized;
        right = right.normalized;
        
        Debug.DrawRay(transform.position + upDir * 0.5f, forward, Color.blue);
        Debug.DrawRay(transform.position + upDir * 0.5f, right, Color.red);

        
        var camForward = _inputReader._moveComposite.y * forward;
        var camRight = _inputReader._moveComposite.x * right;

        
        var camRelativeDirection = camForward + camRight;

        Debug.DrawRay(transform.position + upDir * 0.5f, camRelativeDirection, Color.black);

        Quaternion desiredRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.localRotation;

        transform.localRotation = Quaternion.Lerp(transform.localRotation, desiredRotation, _rotationSpeed * Time.deltaTime);


        if (_inputReader._moveComposite.sqrMagnitude > 0)
        {

            Vector3 cameraEuler = Quaternion.Euler(0, _cameraTransform.eulerAngles.y, 0) * new Vector3(_inputReader._moveComposite.x, 0, _inputReader._moveComposite.y);
            Vector3 movementDirection = cameraEuler.normalized;

            //Quaternion lookRotation = Quaternion.LookRotation(movementDirection, hit.normal);
            //transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, _rotationSpeed * Time.deltaTime);

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
