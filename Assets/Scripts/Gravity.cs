using UnityEngine;

public class Gravity : MonoBehaviour
{
    [SerializeField] private MyCharacterController _characterController;

    [Header("Gravity")]
    [SerializeField] private float _groundedGravity = -0.6f;
    [SerializeField] private float _fallGravity = -30f;
    [SerializeField] private float _maximumFallSpeed = -40f;

    private float _verticalVelocity;
    private float _jumpGravity;
    private bool _isAscendingAfterJump;
    
    public Vector3 AirAcceleration => Vector3.up * _fallGravity;
    public float VerticalVelocity => _verticalVelocity;

    private void Awake()
    {
        Debug.Assert(_characterController != null);
        Debug.Assert(_fallGravity < 0f);
        Debug.Assert(_maximumFallSpeed < 0f);
    }
    
    public void SetVerticalVelocity(float verticalVelocity)
    {
        _verticalVelocity = verticalVelocity;
        if (verticalVelocity > 0f)
            _isAscendingAfterJump = false;
        _characterController.ShouldSnapToGround = false;
    }
    
    public Vector3 CalculateVelocity(float deltaTime)
    {
        if (_characterController.IsGrounded &&
            _verticalVelocity <= 0f)
        {
            _verticalVelocity = _groundedGravity;
            _isAscendingAfterJump = false;
            _characterController.ShouldSnapToGround = true;

            return Vector3.up * _verticalVelocity;
        }

        _characterController.ShouldSnapToGround = false;

        float acceleration =
            _isAscendingAfterJump && _verticalVelocity > 0f
                ? _jumpGravity
                : _fallGravity;

        float velocityBeforeAcceleration =
            _verticalVelocity;

        _verticalVelocity += acceleration * deltaTime;
        _verticalVelocity = Mathf.Max(
            _verticalVelocity,
            _maximumFallSpeed);

        if (_verticalVelocity <= 0f)
            _isAscendingAfterJump = false;

        float averageFrameVelocity =
            (velocityBeforeAcceleration + _verticalVelocity) * 0.5f;

        return Vector3.up * averageFrameVelocity;
    }

    public void StartJump(float jumpSpeed, float jumpHeight)
    {
        jumpHeight = Mathf.Max(jumpHeight, 0.01f);

        _verticalVelocity = jumpSpeed;

        _jumpGravity =
            -(jumpSpeed * jumpSpeed) /
            (2f * jumpHeight);

        _isAscendingAfterJump = true;
        _characterController.ShouldSnapToGround = false;
    }
}