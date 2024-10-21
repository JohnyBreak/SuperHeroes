using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;

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
            //transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            //transform.position = hit.point + upDir * 0.05f;
            _model.forward = -hit.normal;
            StartChange(true, hit.normal, hit.point, upDir);
            
            //ToggleWallMovement(true);
        }
    }


    private void Update()
    {
        //var upDir = _model.TransformDirection(new Vector3(0, 1f, 0));
        //var forward = _model.TransformDirection(new Vector3(0, 0, 1));
        //if (Physics.Raycast(_model.position + upDir, forward + upDir, out var hit, 1, _mask))
        //{
        //    var projectedForward = Project(forward + upDir, hit.normal).normalized;
        //    Debug.DrawRay(_model.position + upDir, forward + upDir, Color.black);
        //    Debug.DrawRay(_model.position + upDir, projectedForward, Color.cyan);
        //}
        

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
            //transform.up = Vector3.up;
            var upDir = transform.TransformDirection(new Vector3(0, 1, 0));
            StartChange(false, Vector3.up, transform.position + upDir * 0.5f, upDir);
            //ToggleWallMovement(false);
        }
    }

    private void ToggleWallMovement(bool wall) 
    {
        //_simpleMovement.enabled = !wall;
        //_wallMovement.enabled = wall;
        //_gravity.enabled = !wall;
        //_ctrl.ShouldSnapToGround = !wall;
        //_playerPivot.enabled = wall;
        //_targetPosition.ToggleWallPosition(wall);
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

    private void StartChange(bool wall, Vector3 hitNormal, Vector3 newPosition, Vector3 upDir) 
    {
        if (_coroutine != null) 
        {
            StopChange();
        }
        _coroutine = StartCoroutine(ChangeOrientationRoutine(wall, hitNormal, newPosition, upDir));
    }

    private void StopChange()
    {
        StopCoroutine(_coroutine);
        _coroutine = null;
    }

    private IEnumerator ChangeOrientationRoutine(bool wall, Vector3 hitNormal, Vector3 newPosition, Vector3 upDir) 
    {
        float currentTime = 0;

        DisableAll();

        var upDir2 = _model.TransformDirection(new Vector3(0, 1f, 0));
        var forward = _model.TransformDirection(new Vector3(0, 0, 1));


        var projectedForward = Project(forward + upDir, hitNormal).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(projectedForward, hitNormal);

        Quaternion startRotation = transform.rotation;

        Vector3 startPosition = transform.position;

        while (currentTime < _changeTime) 
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, newPosition, currentTime / _changeTime);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, currentTime / _changeTime);
            yield return null;
        }

        transform.rotation = Quaternion.FromToRotation(transform.up, hitNormal) * transform.rotation;

        transform.position = newPosition + upDir * 0.05f;

        _playerPivot.SetPivotValues(transform.position);

        ToggleWallMovement(wall);
        
    }

    private Vector3 Project(Vector3 forward, Vector3 normal)
    {
        return forward - Vector3.Dot(forward, normal) * normal;
    }
}
