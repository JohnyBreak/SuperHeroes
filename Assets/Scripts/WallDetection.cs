using Synty.AnimationBaseLocomotion.Samples.InputSystem;
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
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            transform.position = hit.point + upDir * 0.05f;

            _playerPivot.SetPivotValues(transform.position);
            ToggleWallMovement(true);
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
            transform.up = Vector3.up;
            ToggleWallMovement(false);
        }
    }

    private void ToggleWallMovement(bool wall) 
    {
        _simpleMovement.enabled = !wall;
        _wallMovement.enabled = wall;
        _gravity.enabled = !wall;
        _ctrl.ShouldSnapToGround = !wall;
        _playerPivot.enabled = wall;
        _targetPosition.ToggleWallPosition(wall);
    }
}
