using UnityEngine;

public class WebSwingVerlet : MonoBehaviour
{
    [SerializeField] private Gravity _gravity;

    [Header("Rope")]
    [SerializeField] private float _minimumRopeLength = 2f;
    [SerializeField] private float _maximumStretch = 0.4f;

    [Header("Movement")]
    [SerializeField] private float _maximumSpeed = 35f;
    [SerializeField, Range(0.8f, 1f)]
    private float _momentumRetentionAt60Fps = 0.995f;

    private Transform _anchor;
    private Vector3 _previousPosition;
    private float _previousDeltaTime = 1f / 60f;
    private float _ropeLength;
    private bool _isAttached;

    public bool IsAttached => _isAttached;

    public Vector3 Velocity =>
        (transform.position - _previousPosition) /
        Mathf.Max(_previousDeltaTime, 0.0001f);

    private void Awake()
    {
        Debug.Assert(_gravity != null);
    }

    public void Attach(
        Transform anchor,
        float ropeLength,
        Vector3 initialVelocity)
    {
        _anchor = anchor;
        _ropeLength = Mathf.Max(
            ropeLength,
            _minimumRopeLength);

        _previousDeltaTime = Mathf.Max(
            Time.deltaTime,
            1f / 120f);

        _previousPosition =
            transform.position -
            initialVelocity * _previousDeltaTime;

        _isAttached = true;

        // Здесь очистить существующий список wrap-точек:
        // _ropePath.Clear();
    }

    public Vector3 Detach()
    {
        Vector3 releaseVelocity = Velocity;

        _isAttached = false;
        _anchor = null;

        // Здесь очистить wrap-точки:
        // _ropePath.Clear();

        return releaseVelocity;
    }

    public Vector3 CalculateVelocity(float deltaTime)
    {
        if (!_isAttached)
            return Vector3.zero;

        Vector3 currentPosition = transform.position;

        Vector3 acceleration =
            _gravity.AirAcceleration +
            CalculateSwingControlAcceleration();

        float timeRatio =
            deltaTime /
            Mathf.Max(_previousDeltaTime, 0.0001f);

        timeRatio = Mathf.Clamp(
            timeRatio,
            0.25f,
            4f);

        float damping = Mathf.Pow(
            _momentumRetentionAt60Fps,
            deltaTime * 60f);

        Vector3 displacement =
            (currentPosition - _previousPosition) * (timeRatio * damping) +
            acceleration * (deltaTime * deltaTime);

        displacement = Vector3.ClampMagnitude(
            displacement,
            _maximumSpeed * deltaTime);

        Vector3 predictedPosition =
            currentPosition + displacement;

        // Сюда перенеси wrap/unwrap из полного WebSwingVerlet:
        // UpdateRopePath(predictedPosition);

        predictedPosition =
            ConstrainToRope(predictedPosition);

        _previousPosition = currentPosition;
        _previousDeltaTime = deltaTime;

        return
            (predictedPosition - currentPosition) /
            deltaTime;
    }

    private Vector3 ConstrainToRope(
        Vector3 predictedPosition)
    {
        Vector3 pivot = GetActivePivot();

        Vector3 toCharacter =
            predictedPosition - pivot;

        float distance = toCharacter.magnitude;
        float maximumDistance =
            _ropeLength + _maximumStretch;

        if (distance <= maximumDistance ||
            distance < 0.0001f)
        {
            return predictedPosition;
        }

        return pivot +
               toCharacter / distance *
               maximumDistance;
    }

    private Vector3 CalculateSwingControlAcceleration()
    {
        // Здесь остаётся управление раскачиванием
        // из предыдущего WebSwingVerlet.
        return Vector3.zero;
    }

    private Vector3 GetActivePivot()
    {
        // При наличии wrap-точек возвращай последнюю.
        return _anchor.position;
    }
}