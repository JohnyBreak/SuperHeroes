using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _cameraT;
    [SerializeField] private Transform _lookAt;

    [SerializeField] private Transform _horizontal;
    [SerializeField] private Transform _vertical;
    [SerializeField] private float _sensitivity = 100f;

    [SerializeField] private float _maxDistance = 5;
    [SerializeField] private Vector2 _clamp;

    [SerializeField] private float _radius = 0.3f;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private bool _drawGizmos = false;
    
    private Tween _lookAtTween;
    private float _verticalRotation = 0;
    private float _horizontalRotation = 0;
    
    private void HandleRotation()
    {
        var x = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        var y = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        _verticalRotation -= y;

        _verticalRotation = Mathf.Clamp(_verticalRotation, _clamp.x, _clamp.y);

        _horizontalRotation += x;

        if (_horizontalRotation > 360) 
        {
            _horizontalRotation -= 360; 
        }
        if (_horizontalRotation < 0)
        {
            _horizontalRotation = 360 + _horizontalRotation;
        }

        var rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);
        var targetpos = _lookAt.position - (rotation * Vector3.forward * 5);
        _cameraT.position = targetpos;
        //_vertical.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        //_horizontal.localRotation = Quaternion.Euler(0, _horizontalRotation, 0);
    }

    void LateUpdate()
    {
        HandleRotation();
        //HandlePosition();
        //GetCameraCollision();
        var up = _horizontal.TransformDirection(new Vector3(0, 1f, 0));
        //_cameraT.LookAt(_lookAt, up);

        _lookAtTween?.Kill(); 
        
        // Rapidly adapt to the moving target's position
        _lookAtTween = _cameraT.DOLookAt(_lookAt.position, 0.1f, AxisConstraint.None, up); 
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }
    }

    private void HandlePosition()
    {
        _cameraT.position = _vertical.position + Vector3.back * 5;
    }

    private void GetCameraCollision()
    {
        float distance = 5;
        var toCamDir =_cameraT.position - _lookAt.position;
        toCamDir.Normalize();
        //if (Physics.SphereCast(_lookAt.position, _radius, toCamDir.normalized, out var hit, _maxDistance, _mask))
        if (Physics.Raycast(_lookAt.position, toCamDir, out var hit, _maxDistance, _mask)) 
        {
            distance = hit.distance;
        }

        _cameraT.localPosition = Vector3.back * (distance);
    }

    private void OnDrawGizmos()
    {
        if (_drawGizmos == false) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(_lookAt.position, _cameraT.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_cameraT.position, _radius);
    }
}
