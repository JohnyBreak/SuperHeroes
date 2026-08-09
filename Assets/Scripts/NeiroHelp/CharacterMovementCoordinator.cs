using UnityEngine;

public enum CharacterLocomotionMode
{
    Ground,
    Wall,
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

    private CharacterLocomotionMode _mode =
        CharacterLocomotionMode.Ground;

    public CharacterLocomotionMode Mode => _mode;

    private void Awake()
    {
        Debug.Assert(_characterController != null);
        Debug.Assert(_groundMovement != null);
        Debug.Assert(_wallMovement != null);
        Debug.Assert(_gravity != null);
        Debug.Assert(_jump != null);
    }

    public void SetMode(CharacterLocomotionMode mode)
    {
        _mode = mode;

        bool onGround = mode == CharacterLocomotionMode.Ground;
        bool onWall = mode == CharacterLocomotionMode.Wall;

        _groundMovement.enabled = onGround;
        _gravity.enabled = onGround;
        _jump.enabled = onGround;

        _wallMovement.enabled = onWall;

        if (mode == CharacterLocomotionMode.Transition)
        {
            _characterController.ShouldSnapToGround = false;
            return;
        }

        _characterController.ShouldSnapToGround = onGround;
    }

    private void Update()
    {
        if (_mode == CharacterLocomotionMode.Transition)
            return;

        Vector3 velocity = Vector3.zero;

        if (_mode == CharacterLocomotionMode.Ground)
        {
            _jump.Tick();

            velocity =
                _groundMovement.CalculateVelocity() +
                _gravity.CalculateVelocity(Time.deltaTime);
        }
        else if (_mode == CharacterLocomotionMode.Wall)
        {
            velocity = _wallMovement.CalculateVelocity();
        }

        _characterController.Move(velocity);
    }
}