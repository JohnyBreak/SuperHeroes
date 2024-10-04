using UnityEngine;

public class HelpersTransform : MonoBehaviour
{
    [SerializeField] private Transform _upT;
    [SerializeField] private Transform _targetUpT;

    [SerializeField] private Transform _movingT;
    [SerializeField] private Transform _targetT;

    void LateUpdate()
    {
        _movingT.position = _targetT.position;

        //_upT.localRotation = _targetUpT.localRotation;
        _upT.localRotation = Quaternion.Lerp(_upT.localRotation, _targetUpT.localRotation, 4f * Time.deltaTime);
    }
}
