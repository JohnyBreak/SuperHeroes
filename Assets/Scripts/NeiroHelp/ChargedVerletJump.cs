using UnityEngine;

public class ChargedJump : MonoBehaviour
{
    [SerializeField] private MyCharacterController _characterController;
    [SerializeField] private Gravity _gravity;

    [Header("Jump profile")]
    [SerializeField] private float _jumpSpeed = 12f;
    [SerializeField] private float _minimumJumpHeight = 1f;
    [SerializeField] private float _maximumJumpHeight = 5f;

    [Header("Charging")]
    [SerializeField] private float _maximumChargeDuration = 1f;
    [SerializeField] private float _tapDuration = 0.1f;

    private float _chargeStartTime;
    private bool _isCharging;

    public bool IsCharging => _isCharging;

    public float Charge01
    {
        get
        {
            if (!_isCharging)
                return 0f;

            return CalculateCharge(
                Time.time - _chargeStartTime);
        }
    }

    private void Awake()
    {
        Debug.Assert(_characterController != null);
        Debug.Assert(_gravity != null);
        Debug.Assert(_jumpSpeed > 0f);
        Debug.Assert(_minimumJumpHeight > 0f);
        Debug.Assert(_maximumJumpHeight >= _minimumJumpHeight);
        Debug.Assert(_maximumChargeDuration > _tapDuration);
    }

    public void Tick()
    {
        if (_characterController.IsGrounded &&
            Input.GetButtonDown("Jump"))
        {
            _isCharging = true;
            _chargeStartTime = Time.time;
        }

        if (!_isCharging)
            return;

        if (!_characterController.IsGrounded)
        {
            _isCharging = false;
            return;
        }

        if (Input.GetButtonUp("Jump"))
            PerformJump();
    }

    private void PerformJump()
    {
        float heldDuration =
            Time.time - _chargeStartTime;

        float charge = CalculateCharge(heldDuration);

        float jumpHeight = Mathf.Lerp(
            _minimumJumpHeight,
            _maximumJumpHeight,
            charge);

        _gravity.StartJump(
            _jumpSpeed,
            jumpHeight);

        _isCharging = false;
    }
    
    private float CalculateCharge(float heldDuration)
    {
        float effectiveDuration = Mathf.Max(
            0f,
            heldDuration - _tapDuration);

        float chargeDuration =
            _maximumChargeDuration - _tapDuration;

        return Mathf.Clamp01(
            effectiveDuration / chargeDuration);
    }
}