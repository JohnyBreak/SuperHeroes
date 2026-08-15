using System;
using System.Collections.Generic;
using UnityEngine;

public class WebSwingVerlet : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Gravity _gravity;
    [SerializeField] private Transform _viewTransform;
    [SerializeField] private LineRenderer _lineRenderer;

    [Header("Rope")]
    [SerializeField] private float _minimumRopeLength = 2f;
    [SerializeField] private float _maximumRopeLength = 25f;
    [SerializeField] private float _retractSpeed = 5f;
    [SerializeField] private float _extendSpeed = 3f;
    [SerializeField] private float _maximumStretch = 0.4f;
    [SerializeField] private float _stretchStrength = 60f;
    [SerializeField] private float _stretchDamping = 8f;

    [Header("Motion")]
    [SerializeField] private float _maximumSpeed = 35f;
    [SerializeField, Range(0.8f, 1f)]
    private float _momentumRetentionAt60Fps = 0.995f;

    [Header("Swing control")]
    [SerializeField] private float _steeringAcceleration = 10f;
    [SerializeField] private float _pumpingAcceleration = 18f;
    [SerializeField] private float _stationarySpeedThreshold = 0.5f;

    [Header("Rope collision")]
    [SerializeField] private LayerMask _ropeCollisionMask;
    [SerializeField] private float _ropeCollisionRadius = 0.04f;
    [SerializeField] private float _wrapSurfaceOffset = 0.06f;
    [SerializeField] private float _minimumWrapSegmentLength = 0.15f;
    [SerializeField] private float _unwrapDelay = 0.1f;
    [SerializeField] private int _maximumWrapPoints = 12;
    [SerializeField] private int _maximumWrapsPerFrame = 3;

    [Header("Input")]
    [SerializeField] private KeyCode _retractKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode _extendKey = KeyCode.LeftControl;

    private readonly RopePath _ropePath = new();

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
        Debug.Assert(_viewTransform != null);

        _previousPosition = transform.position;
    }

    public void Attach(
        Transform anchor,
        float ropeLength,
        Vector3 initialVelocity)
    {
        if (anchor == null)
            throw new ArgumentNullException(nameof(anchor));

        _anchor = anchor;
        _ropePath.Clear();

        float distanceToAnchor = Vector3.Distance(
            transform.position,
            anchor.position);

        _ropeLength = Mathf.Clamp(
            Mathf.Max(ropeLength, distanceToAnchor, _minimumRopeLength),
            _minimumRopeLength,
            _maximumRopeLength);

        _previousDeltaTime = Mathf.Max(Time.deltaTime, 1f / 120f);
        _previousPosition =
            transform.position -
            initialVelocity * _previousDeltaTime;

        _isAttached = true;
        RefreshRopeVisualization();
    }

    public Vector3 Detach()
    {
        Vector3 releaseVelocity = Velocity;

        _isAttached = false;
        _anchor = null;
        _ropePath.Clear();
        RefreshRopeVisualization();

        return releaseVelocity;
    }

    public Vector3 CalculateVelocity(float deltaTime)
    {
        if (!_isAttached || deltaTime <= 0f)
            return Vector3.zero;

        ProcessRopeLengthInput(deltaTime);

        Vector3 currentPosition = transform.position;

        Vector3 acceleration =
            _gravity.AirAcceleration +
            CalculateSwingControlAcceleration() +
            CalculateStretchAcceleration(currentPosition);

        float timeRatio =
            deltaTime / Mathf.Max(_previousDeltaTime, 0.0001f);
        timeRatio = Mathf.Clamp(timeRatio, 0.25f, 4f);

        float damping = Mathf.Pow(
            _momentumRetentionAt60Fps,
            deltaTime * 60f);

        Vector3 displacement =
            (currentPosition - _previousPosition) * (timeRatio * damping) +
            acceleration * (deltaTime * deltaTime);

        displacement = Vector3.ClampMagnitude(
            displacement,
            _maximumSpeed * deltaTime);

        Vector3 predictedPosition = currentPosition + displacement;

        UpdateRopePath(predictedPosition);
        predictedPosition = ConstrainToRope(predictedPosition);

        _previousPosition = currentPosition;
        _previousDeltaTime = deltaTime;

        RefreshRopeVisualization(predictedPosition);

        return (predictedPosition - currentPosition) / deltaTime;
    }

    private void ProcessRopeLengthInput(float deltaTime)
    {
        float minimumTotal =
            _ropePath.UsedLength + _minimumRopeLength;

        if (Input.GetKey(_retractKey))
        {
            _ropeLength = Mathf.Max(
                minimumTotal,
                _ropeLength - _retractSpeed * deltaTime);
        }

        if (Input.GetKey(_extendKey))
        {
            _ropeLength = Mathf.Min(
                _maximumRopeLength,
                _ropeLength + _extendSpeed * deltaTime);
        }
    }

    private Vector3 CalculateSwingControlAcceleration()
    {
        Vector3 ropeDirection = GetRopeDirection(transform.position);
        Vector3 tangentialVelocity =
            Vector3.ProjectOnPlane(Velocity, ropeDirection);

        GetViewAxes(ropeDirection, out Vector3 forward, out Vector3 right);

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 acceleration =
            right * horizontalInput * _steeringAcceleration;

        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            Vector3 pumpingDirection =
                forward * Mathf.Sign(verticalInput);

            float pumpingFactor = CalculatePumpingFactor(
                pumpingDirection,
                tangentialVelocity);

            acceleration +=
                pumpingDirection *
                _pumpingAcceleration *
                pumpingFactor;
        }

        return Vector3.ProjectOnPlane(acceleration, ropeDirection);
    }

    private float CalculatePumpingFactor(
        Vector3 pumpingDirection,
        Vector3 tangentialVelocity)
    {
        float speed = tangentialVelocity.magnitude;

        if (speed < _stationarySpeedThreshold)
            return 1f;

        float alignment = Vector3.Dot(
            pumpingDirection,
            tangentialVelocity / speed);

        return Mathf.Clamp01(alignment);
    }

    private void GetViewAxes(
        Vector3 ropeDirection,
        out Vector3 forward,
        out Vector3 right)
    {
        forward = Vector3.ProjectOnPlane(
            _viewTransform.forward,
            ropeDirection);

        right = Vector3.ProjectOnPlane(
            _viewTransform.right,
            ropeDirection);

        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.Cross(ropeDirection, _viewTransform.right);

        if (right.sqrMagnitude < 0.0001f)
            right = Vector3.Cross(forward, ropeDirection);

        forward.Normalize();
        right.Normalize();
    }

    private Vector3 CalculateStretchAcceleration(Vector3 position)
    {
        Vector3 pivot = GetActivePivot();
        Vector3 toCharacter = position - pivot;
        float distance = toCharacter.magnitude;
        float freeLength = GetFreeRopeLength();

        if (distance <= freeLength || distance < 0.0001f)
            return Vector3.zero;

        Vector3 ropeDirection = toCharacter / distance;
        float stretch = Mathf.Min(distance - freeLength, _maximumStretch);
        float outwardSpeed = Mathf.Max(
            0f,
            Vector3.Dot(Velocity, ropeDirection));

        float pull =
            stretch * _stretchStrength +
            outwardSpeed * _stretchDamping;

        return -ropeDirection * pull;
    }

    private void UpdateRopePath(Vector3 characterPosition)
    {
        Vector3 anchorPosition = _anchor.position;

        _ropePath.RemoveClearWrapPoints(
            anchorPosition,
            characterPosition,
            Time.time,
            _unwrapDelay,
            IsRopeSegmentClear);

        _ropePath.AddBlockingWrapPoints(
            anchorPosition,
            characterPosition,
            Time.time,
            GetFreeRopeLength(),
            _minimumRopeLength,
            _maximumWrapPoints,
            _maximumWrapsPerFrame,
            TryFindWrapPoint);
    }

    private Vector3 ConstrainToRope(Vector3 predictedPosition)
    {
        Vector3 pivot = GetActivePivot();
        Vector3 toCharacter = predictedPosition - pivot;
        float distance = toCharacter.magnitude;
        float maximumDistance = GetFreeRopeLength() + _maximumStretch;

        if (distance <= maximumDistance || distance < 0.0001f)
            return predictedPosition;

        return pivot + toCharacter / distance * maximumDistance;
    }

    private bool TryFindWrapPoint(
        Vector3 pivot,
        Vector3 characterPosition,
        out Vector3 wrapPosition,
        out float segmentLength)
    {
        wrapPosition = default;
        segmentLength = 0f;

        Vector3 toCharacter = characterPosition - pivot;
        float distance = toCharacter.magnitude;

        if (distance <= _minimumWrapSegmentLength)
            return false;

        float castOffset = _ropeCollisionRadius + _wrapSurfaceOffset;
        if (distance <= castOffset * 2f)
            return false;

        Vector3 direction = toCharacter / distance;

        if (!Physics.SphereCast(
                pivot + direction * castOffset,
                _ropeCollisionRadius,
                direction,
                out RaycastHit hit,
                distance - castOffset * 2f,
                _ropeCollisionMask,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        wrapPosition = hit.point + hit.normal * castOffset;
        segmentLength = Vector3.Distance(pivot, wrapPosition);

        return segmentLength >= _minimumWrapSegmentLength;
    }

    private bool IsRopeSegmentClear(Vector3 start, Vector3 end)
    {
        Vector3 segment = end - start;
        float distance = segment.magnitude;
        float castOffset = _ropeCollisionRadius + _wrapSurfaceOffset;

        if (distance <= castOffset * 2f)
            return true;

        Vector3 direction = segment / distance;

        return !Physics.SphereCast(
            start + direction * castOffset,
            _ropeCollisionRadius,
            direction,
            out _,
            distance - castOffset * 2f,
            _ropeCollisionMask,
            QueryTriggerInteraction.Ignore);
    }

    private Vector3 GetActivePivot()
    {
        return _ropePath.GetActivePivot(_anchor.position);
    }

    private Vector3 GetRopeDirection(Vector3 characterPosition)
    {
        Vector3 direction = characterPosition - GetActivePivot();

        if (direction.sqrMagnitude < 0.0001f)
            return Vector3.down;

        return direction.normalized;
    }

    private float GetFreeRopeLength()
    {
        return Mathf.Max(
            _minimumRopeLength,
            _ropeLength - _ropePath.UsedLength);
    }

    private void RefreshRopeVisualization()
    {
        RefreshRopeVisualization(transform.position);
    }

    private void RefreshRopeVisualization(Vector3 characterPosition)
    {
        if (_lineRenderer == null)
            return;

        if (!_isAttached)
        {
            _lineRenderer.positionCount = 0;
            return;
        }

        _ropePath.WriteLinePositions(
            _lineRenderer,
            _anchor.position,
            characterPosition);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_isAttached || _anchor == null)
            return;

        Gizmos.color = Color.cyan;
        _ropePath.DrawGizmos(_anchor.position, transform.position);
    }

    private sealed class RopePath
    {
        private readonly List<WrapPoint> _wrapPoints = new();

        public float UsedLength
        {
            get
            {
                float length = 0f;
                for (int i = 0; i < _wrapPoints.Count; i++)
                    length += _wrapPoints[i].SegmentLength;
                return length;
            }
        }

        public void Clear()
        {
            _wrapPoints.Clear();
        }

        public Vector3 GetActivePivot(Vector3 anchorPosition)
        {
            if (_wrapPoints.Count == 0)
                return anchorPosition;

            return _wrapPoints[_wrapPoints.Count - 1].Position;
        }

        public void RemoveClearWrapPoints(
            Vector3 anchorPosition,
            Vector3 characterPosition,
            float currentTime,
            float unwrapDelay,
            Func<Vector3, Vector3, bool> isSegmentClear)
        {
            while (_wrapPoints.Count > 0)
            {
                int lastIndex = _wrapPoints.Count - 1;
                WrapPoint wrapPoint = _wrapPoints[lastIndex];

                if (currentTime - wrapPoint.CreationTime < unwrapDelay)
                    return;

                Vector3 previousPivot = lastIndex > 0
                    ? _wrapPoints[lastIndex - 1].Position
                    : anchorPosition;

                if (!isSegmentClear(previousPivot, characterPosition))
                    return;

                _wrapPoints.RemoveAt(lastIndex);
            }
        }

        public void AddBlockingWrapPoints(
            Vector3 anchorPosition,
            Vector3 characterPosition,
            float currentTime,
            float freeLength,
            float minimumFreeLength,
            int maximumWrapPoints,
            int maximumWrapsPerFrame,
            WrapPointProbe wrapPointProbe)
        {
            for (int i = 0; i < maximumWrapsPerFrame; i++)
            {
                if (_wrapPoints.Count >= maximumWrapPoints)
                    return;

                Vector3 pivot = GetActivePivot(anchorPosition);

                if (!wrapPointProbe(
                        pivot,
                        characterPosition,
                        out Vector3 wrapPosition,
                        out float segmentLength))
                {
                    return;
                }

                if (freeLength - segmentLength < minimumFreeLength)
                    return;

                _wrapPoints.Add(new WrapPoint
                {
                    Position = wrapPosition,
                    SegmentLength = segmentLength,
                    CreationTime = currentTime
                });

                freeLength -= segmentLength;
            }
        }

        public void WriteLinePositions(
            LineRenderer lineRenderer,
            Vector3 anchorPosition,
            Vector3 characterPosition)
        {
            int positionCount = _wrapPoints.Count + 2;
            lineRenderer.positionCount = positionCount;
            lineRenderer.SetPosition(0, anchorPosition);

            for (int i = 0; i < _wrapPoints.Count; i++)
                lineRenderer.SetPosition(i + 1, _wrapPoints[i].Position);

            lineRenderer.SetPosition(positionCount - 1, characterPosition);
        }

        public void DrawGizmos(
            Vector3 anchorPosition,
            Vector3 characterPosition)
        {
            Vector3 previousPosition = anchorPosition;

            for (int i = 0; i < _wrapPoints.Count; i++)
            {
                Vector3 wrapPosition = _wrapPoints[i].Position;
                Gizmos.DrawLine(previousPosition, wrapPosition);
                Gizmos.DrawSphere(wrapPosition, 0.08f);
                previousPosition = wrapPosition;
            }

            Gizmos.DrawLine(previousPosition, characterPosition);
        }

        private struct WrapPoint
        {
            public Vector3 Position;
            public float SegmentLength;
            public float CreationTime;
        }
    }

    private delegate bool WrapPointProbe(
        Vector3 pivot,
        Vector3 characterPosition,
        out Vector3 wrapPosition,
        out float segmentLength);
}