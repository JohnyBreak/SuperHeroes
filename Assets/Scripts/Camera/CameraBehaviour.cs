using TMPro;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform _cameraT;
    [SerializeField] private Transform _lookAt;

    [SerializeField] private Transform _horizontal;
    [SerializeField] private Transform _vertical;
    [SerializeField] private float _sensitivity = 100f;
    [SerializeField] private Vector2 _clamp;

    [SerializeField] private float _radius = 0.3f;
    [SerializeField] private LayerMask _mask;

    private float _verticalRotation = 0;
    private float _horizontalRotation = 0;
    private float _distance = 5;
    private Vector3 _up;
    
    private void HandleRotation()
    {
        var x = Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        var y = Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        _verticalRotation -= y;

        _verticalRotation = Mathf.Clamp(_verticalRotation, _clamp.x, _clamp.y);

        _horizontalRotation += x;
        _vertical.localRotation = Quaternion.Euler(_verticalRotation, 0, 0);
        _horizontal.localRotation = Quaternion.Euler(0, _horizontalRotation, 0);
    }

    private void Update()
    {
        _up = _horizontal.TransformDirection(new Vector3(0, 1f, 0));
    }

    void LateUpdate()
    {
        HandleRotation();
        GetCameraCollision();
        _cameraT.LookAt(_lookAt, _up);
    }

    private void GetCameraCollision()
    {
        _distance = 5;
        var toCamDir =_cameraT.position - _lookAt.position;
        toCamDir.Normalize();
        if (Physics.SphereCast(_lookAt.position, _radius, toCamDir, out var hit, 5, _mask))
        {
            _distance = hit.distance;
        }

        _cameraT.localPosition = Vector3.back * _distance;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(_lookAt.position, _cameraT.position);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_cameraT.position, _radius);
    }
}
