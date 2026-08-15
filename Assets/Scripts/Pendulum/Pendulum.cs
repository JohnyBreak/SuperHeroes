using UnityEngine;

public class Pendulum : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private float _camSpeed = 10;
    
    [SerializeField] private Transform _root;
    [SerializeField] private Transform _cube;
    [SerializeField] private Transform _cubeModel;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _maxFallGravity = -35f;
    [SerializeField] private float _maxDistance = 10;
    [SerializeField] private float _steeringSpeed = 1;
    
    private Vector3 _previousPosition, _previousVelocity;
    private bool _connected;
    
    private void Start()
    {
        _previousPosition = _cube.position;
    }

    private void Update()
    {
        var newCamPos = _root.position + Vector3.right * -30;
        //_camera.position = _root.position + Vector3.right * -30;
        _camera.position = Vector3.Lerp(_camera.position, newCamPos, _camSpeed * Time.deltaTime);
        
        _connected = Input.GetKey(KeyCode.Mouse0);
        _lineRenderer.enabled = _connected;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _root.position = _cube.position + _cube.up * 8 + _cube.forward * 5;
        }

        HandleMovement();
        Rotate();
        UpdateLine();
    }

    private void Rotate()
    {
        _cubeModel.up = (_root.position - _cube.position).normalized;
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

        Vector3 inputAcceleration = (_cube.forward * (input.z * _steeringSpeed)) +
                                    (_cube.right * (input.x * _steeringSpeed));
        
        Vector3 currentPosition = _cube.position;
        
        Vector3 acceleration = (Vector3.up * _gravity) + inputAcceleration;// add XZ steering
        
        Vector3 predictedPosition = currentPosition + 
                                    (currentPosition - _previousPosition) +
                                    acceleration * (Time.deltaTime * Time.deltaTime);
        if (_connected)
        {
            predictedPosition = ConstrainToRope(predictedPosition);
        }

        _previousPosition = currentPosition;
        
        Vector3 velocity = (predictedPosition - currentPosition);// / Time.deltaTime;
        
        _previousVelocity = velocity;
        _cube.position += velocity;
    }
    
    private void UpdateLine()
    {
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, _root.position);
        _lineRenderer.SetPosition(1, _cube.position);
    }

    private Vector3 ConstrainToRope(Vector3 predictedPosition)
    {
        Vector3 pivot = _root.position;
        Vector3 toCharacter = predictedPosition - pivot;
        float distance = toCharacter.magnitude;

        if (distance <= _maxDistance || distance < 0.0001f)
        {
            return predictedPosition;
        }

        return pivot + toCharacter / distance * _maxDistance;
    }
}