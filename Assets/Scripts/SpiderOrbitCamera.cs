using UnityEngine;

public class SpiderOrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _focusOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float _distance = 5f;
    [SerializeField] private float _sensitivity = 200f;
    [SerializeField] private float _pitchMin = -80f;
    [SerializeField] private float _pitchMax = 80f;
    [SerializeField] private float _upAlignSpeed = 8f;

    [Header("Collision")]
    [SerializeField] private LayerMask _collisionMask = ~0;
    [SerializeField] private float _collisionRadius = 0.2f;
    [SerializeField] private float _collisionBuffer = 0.05f;
    
    [SerializeField] private bool _hideCursor = true;
    private float _yaw;
    private float _pitch;
    private Quaternion _orbitFrame = Quaternion.identity;

    private void Awake()
    {
        Debug.Assert(_target != null, $"{nameof(SpiderOrbitCamera)} needs a target.");
        if (_hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    
    private void Start()
    {
        _orbitFrame = BuildOrbitFrame(_target.up, -_target.forward);
return;
        Vector3 focusPoint = _target.TransformPoint(_focusOffset);
        Vector3 localOffset = Quaternion.Inverse(_orbitFrame) * (transform.position - focusPoint);

        if (localOffset.sqrMagnitude > 0.0001f)
        {
            Vector3 direction = localOffset.normalized;
            _yaw = Mathf.Atan2(direction.x, -direction.z) * Mathf.Rad2Deg;
            _pitch = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
            _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
            _distance = localOffset.magnitude;
        }
    }

    private void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        _yaw += mouseX;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);

        Quaternion targetFrame = BuildOrbitFrame(_target.up, _orbitFrame * Vector3.forward);
        float blend = 1f - Mathf.Exp(-_upAlignSpeed * Time.deltaTime);
        _orbitFrame = Quaternion.Slerp(_orbitFrame, targetFrame, blend);

        Quaternion orbitRotation =
            _orbitFrame *
            Quaternion.AngleAxis(_yaw, Vector3.up) *
            Quaternion.AngleAxis(_pitch, Vector3.right);

        Vector3 focusPoint = _target.TransformPoint(_focusOffset);
        Vector3 desiredDirection = orbitRotation * Vector3.back;
        float distance = GetCollisionDistance(focusPoint, desiredDirection, _distance);

        Vector3 offset = desiredDirection * distance;
        transform.position = focusPoint + offset;
        transform.rotation = Quaternion.LookRotation(-offset, _orbitFrame * Vector3.up);
    }

    private float GetCollisionDistance(Vector3 focusPoint, Vector3 direction, float desiredDistance)
    {
        float castDistance = desiredDistance - _collisionBuffer;
        if (castDistance <= 0f)
            return _collisionBuffer;

        if (Physics.SphereCast(
                focusPoint,
                _collisionRadius,
                direction,
                out RaycastHit hit,
                castDistance,
                _collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(hit.distance - _collisionBuffer, _collisionBuffer);
        }

        return desiredDistance;
    }

    private static Quaternion BuildOrbitFrame(Vector3 up, Vector3 approximateForward)
    {
        up.Normalize();

        Vector3 forward = Vector3.ProjectOnPlane(approximateForward, up);
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.ProjectOnPlane(Vector3.forward, up);
            if (forward.sqrMagnitude < 0.0001f)
                forward = Vector3.ProjectOnPlane(Vector3.right, up);
        }

        return Quaternion.LookRotation(forward.normalized, up);
    }
}