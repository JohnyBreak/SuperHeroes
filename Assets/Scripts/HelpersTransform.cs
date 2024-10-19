using UnityEngine;

public class HelpersTransform : MonoBehaviour
{
    [SerializeField] private Transform _upT;
    [SerializeField] private Transform _targetUpT;

    [SerializeField] private Transform _movingT;
    [SerializeField] private Transform _targetT;
    [SerializeField] private float _rotationSpeed = 1f;

    private void Update()
    {
        _movingT.position = _targetT.position;

        var step = _rotationSpeed * Time.deltaTime;

        _upT.rotation = Quaternion.Lerp(_upT.rotation, _targetUpT.rotation, step);
    }
}
