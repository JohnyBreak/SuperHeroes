using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using System.Collections;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Transform _model;

    [SerializeField] private MonoBehaviour _simpleMovement;
    [SerializeField] private MonoBehaviour _wallMovement;
    [SerializeField] private MonoBehaviour _gravity;
    [SerializeField] private MyCharacterController _ctrl;
    [SerializeField] private PlayerPivot _playerPivot;
    [SerializeField] private CameraTargetPosition _targetPosition;
    private bool _onWall => _wallMovement.enabled;
    private float _dot;
    private Coroutine _coroutine;
    [SerializeField] private float _changeTime = 0.3f;

    private void Start()
    {
        _inputReader.onJumpPerformed += CheckWall;
        _inputReader.onCrouchActivated += ReleaseWall;
    }

    private void OnDestroy()
    {
        _inputReader.onJumpPerformed -= CheckWall;
        _inputReader.onCrouchActivated -= ReleaseWall;
    }

    private void CheckWall()
    {
        var upDir = _model.TransformDirection(new Vector3(0, 1f, 0));

        if (Physics.Raycast(_model.position + upDir, _model.forward, out var hit, 1, _mask))
        {
            _model.forward = -hit.normal;

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            StartChange(true, hit.point, targetRotation);
        }
    }


    private void Update()
    {
        if (_onWall)
        {
            var dir = transform.TransformDirection(new Vector3(0, -1, 0));
            if (Physics.Raycast(transform.position + dir * -0.5f, dir, 1, _mask))
            {
                if (IsFloorNormal())
                {
                    ReleaseWall();
                }
            }
        }
    }

    private bool IsFloorNormal()
    {
        _dot = Vector3.Dot(_playerPivot.Pivot.up, Vector3.up);
        return (_dot <= 1 && _dot > 0.85f);
    }

    private void ReleaseWall()
    {
        if (_wallMovement.enabled)
        {
            var upDir = transform.TransformDirection(new Vector3(0, 1, 0));
            StartChange(false, transform.position + upDir * 0.5f, Quaternion.identity);
        }
    }

    private void ToggleWallMovement(bool wall)
    {
        ToggleWalkObjects(wall);
        ToggleWallObjects(wall);
    }

    private void ToggleWalkObjects(bool wall)
    {
        _simpleMovement.enabled = !wall;

        _gravity.enabled = !wall;
        _ctrl.ShouldSnapToGround = !wall;
    }

    private void ToggleWallObjects(bool wall)
    {
        _wallMovement.enabled = wall;
        _playerPivot.enabled = wall;
        _targetPosition.ToggleWallPosition(wall);
    }
    private void DisableAll()
    {
        _simpleMovement.enabled = false;
        _wallMovement.enabled = false;
        _gravity.enabled = false;
        _ctrl.ShouldSnapToGround = false;
        _playerPivot.enabled = false;
        _targetPosition.ToggleWallPosition(false);
    }

    private void StartChange(bool wall, Vector3 newPosition, Quaternion newRotation)
    {
        if (_coroutine != null)
        {
            StopChange();
        }

        _coroutine = StartCoroutine(ChangeOrientationRoutine(wall, newPosition, newRotation));
    }

    private void StopChange()
    {
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private IEnumerator ChangeOrientationRoutine(bool wall, Vector3 newPosition, Quaternion newRotation)
    {
        float currentTime = 0;

        DisableAll();

        Quaternion targetRotation = newRotation;

        Quaternion startRotation = transform.rotation;

        Vector3 startPosition = transform.position;

        while (currentTime < _changeTime)
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, newPosition, currentTime / _changeTime);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, currentTime / _changeTime);
            yield return null;
        }

        transform.rotation = newRotation;

        transform.position = newPosition;

        _playerPivot.SetPivotValues(transform.position);

        ToggleWallMovement(wall);
    }
}
