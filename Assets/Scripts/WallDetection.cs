using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using System.Collections;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Transform _model;
    [SerializeField] private CharacterMovementCoordinator _coordinator;
    [SerializeField] private PlayerPivot _playerPivot;
    [SerializeField] private CameraTargetPosition _targetPosition;
    [SerializeField] private float _changeTime = 0.3f;

    private Coroutine _changeRoutine;

    private void Start()
    {
        _inputReader.onWallCheckPerformed += CheckWall;
        _inputReader.onCrouchActivated += ReleaseWall;

        _coordinator.SetMode(CharacterLocomotionMode.Ground);
    }

    private void OnDestroy()
    {
        _inputReader.onWallCheckPerformed -= CheckWall;
        _inputReader.onCrouchActivated -= ReleaseWall;
    }

    private void Update()
    {
        if (_coordinator.Mode != CharacterLocomotionMode.Wall)
            return;

        Vector3 downDirection = -transform.up;
        Vector3 origin =
            transform.position - downDirection * 0.5f;

        if (!Physics.Raycast(
                origin,
                downDirection,
                1f,
                _mask))
        {
            return;
        }

        if (IsFloorNormal())
            ReleaseWall();
    }

    private void CheckWall()
    {
        if (_coordinator.Mode != CharacterLocomotionMode.Ground)
            return;

        Vector3 upDirection = _model.up;

        if (!Physics.Raycast(
                _model.position + upDirection,
                _model.forward,
                out RaycastHit hit,
                1f,
                _mask))
        {
            return;
        }

        _model.forward = -hit.normal;

        Quaternion targetRotation =
            Quaternion.FromToRotation(
                transform.up,
                hit.normal) *
            transform.rotation;

        StartChange(
            CharacterLocomotionMode.Wall,
            hit.point,
            targetRotation);
    }

    private void ReleaseWall()
    {
        if (_coordinator.Mode != CharacterLocomotionMode.Wall)
            return;

        Vector3 releasePosition =
            transform.position + transform.up * 0.5f;

        StartChange(
            CharacterLocomotionMode.Ground,
            releasePosition,
            Quaternion.identity);
    }

    private bool IsFloorNormal()
    {
        float alignment = Vector3.Dot(
            _playerPivot.Pivot.up,
            Vector3.up);

        return alignment > 0.85f;
    }

    private void StartChange(
        CharacterLocomotionMode nextMode,
        Vector3 newPosition,
        Quaternion newRotation)
    {
        if (_changeRoutine != null)
            StopCoroutine(_changeRoutine);

        _changeRoutine = StartCoroutine(
            ChangeOrientationRoutine(
                nextMode,
                newPosition,
                newRotation));
    }

    private IEnumerator ChangeOrientationRoutine(
        CharacterLocomotionMode nextMode,
        Vector3 newPosition,
        Quaternion newRotation)
    {
        _coordinator.SetMode(
            CharacterLocomotionMode.Transition);

        _targetPosition.ToggleWallPosition(
            nextMode == CharacterLocomotionMode.Wall);

        Quaternion startRotation = transform.rotation;
        Vector3 startPosition = transform.position;
        float currentTime = 0f;

        while (currentTime < _changeTime)
        {
            currentTime += Time.deltaTime;
            float progress = currentTime / _changeTime;

            transform.position = Vector3.Lerp(
                startPosition,
                newPosition,
                progress);

            transform.rotation = Quaternion.Lerp(
                startRotation,
                newRotation,
                progress);

            yield return null;
        }

        transform.position = newPosition;
        transform.rotation = newRotation;

        _playerPivot.SetPivotValues(transform.position);
        _playerPivot.enabled =
            nextMode == CharacterLocomotionMode.Wall;

        _coordinator.SetMode(nextMode);
        _changeRoutine = null;
    }
}