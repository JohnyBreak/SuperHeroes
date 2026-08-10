using UnityEngine;

public enum CharacterLocomotionMode
{
    Ground,
    Wall,
    Swing,
    Transition
}

[DefaultExecutionOrder(100)]
public class CharacterMovementCoordinator : MonoBehaviour
{
    [SerializeField] private MyCharacterController _characterController;
    [SerializeField] private SimpleMovement _groundMovement;
    [SerializeField] private WallMovement _wallMovement;
    [SerializeField] private Gravity _gravity;
    [SerializeField] private ChargedJump _jump;
    [SerializeField] private WebSwingVerlet _webSwing;
    [SerializeField] private Transform _swingAnchor;
    [SerializeField] private Transform _camera;
    
    private CharacterLocomotionMode _mode =
        CharacterLocomotionMode.Ground;

    private Vector3 _lastVelocity;

    public CharacterLocomotionMode Mode => _mode;

    private void Awake()
    {
        Debug.Assert(_characterController != null);
        Debug.Assert(_groundMovement != null);
        Debug.Assert(_wallMovement != null);
        Debug.Assert(_gravity != null);
        Debug.Assert(_jump != null);
        Debug.Assert(_webSwing != null);
    }

    private void Update()
    {
        if (_swingAnchor != null && Input.GetKeyDown(KeyCode.Mouse0))
        {
            _swingAnchor.position = transform.position + (_camera.transform.forward + Vector3.up).normalized * 5;
            StartSwing(_swingAnchor, 5);
        }
        if (_swingAnchor != null && Input.GetKeyUp(KeyCode.Mouse0))
        {
            //_swingAnchor.position = transform.position + _camera.transform.forward * 5 + Vector3.up * 5;
            StopSwing();
        }

        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        _lastVelocity = CalculateVelocity(deltaTime);
        _characterController.Move(_lastVelocity);
    }

    public void StartSwing(
        Transform anchor,
        float ropeLength)
    {
        if (_mode == CharacterLocomotionMode.Transition)
            return;

        _webSwing.Attach(
            anchor,
            ropeLength,
            _lastVelocity);

        _characterController.ShouldSnapToGround = false;
        _mode = CharacterLocomotionMode.Swing;
    }

    public void StopSwing()
    {
        if (_mode != CharacterLocomotionMode.Swing)
            return;

        Vector3 releaseVelocity =
            _webSwing.Detach();

        _gravity.SetVerticalVelocity(
            Vector3.Dot(
                releaseVelocity,
                Vector3.up));

        _groundMovement.SetHorizontalVelocity(
            releaseVelocity);

        _lastVelocity = releaseVelocity;
        _mode = CharacterLocomotionMode.Ground;
    }

    public void SetMode(
        CharacterLocomotionMode mode)
    {
        if (_mode == CharacterLocomotionMode.Swing &&
            mode != CharacterLocomotionMode.Swing)
        {
            StopSwing();
        }

        _mode = mode;

        _characterController.ShouldSnapToGround =
            mode == CharacterLocomotionMode.Ground;
    }

    private Vector3 CalculateVelocity(float deltaTime)
    {
        switch (_mode)
        {
            case CharacterLocomotionMode.Ground:
                return CalculateGroundVelocity(deltaTime);

            case CharacterLocomotionMode.Wall:
                return _wallMovement.CalculateVelocity();

            case CharacterLocomotionMode.Swing:
                return _webSwing.CalculateVelocity(deltaTime);

            case CharacterLocomotionMode.Transition:
            default:
                return Vector3.zero;
        }
    }

    private Vector3 CalculateGroundVelocity(
        float deltaTime)
    {
        _jump.Tick();

        Vector3 verticalVelocity =
            _gravity.CalculateVelocity(deltaTime);

        bool useGroundControl =
            _characterController.IsGrounded &&
            _gravity.VerticalVelocity <= 0f;

        Vector3 horizontalVelocity =
            _groundMovement.CalculateVelocity(
                deltaTime,
                useGroundControl);

        return
            horizontalVelocity +
            verticalVelocity;
    }
}