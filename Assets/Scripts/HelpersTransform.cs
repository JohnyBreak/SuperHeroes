using UnityEngine;

public class HelpersTransform : MonoBehaviour
{
    [SerializeField] private Transform _upT;
    [SerializeField] private Transform _targetUpT;

    [SerializeField] private Transform _movingT;
    [SerializeField] private Transform _targetT;

    private void Update()
    {
        _movingT.position = _targetT.position;

        //_upT.rotation = _targetUpT.rotation;

        _upT.rotation = Quaternion.Lerp(_upT.rotation, _targetUpT.rotation, 6f * Time.deltaTime);
    }
}
