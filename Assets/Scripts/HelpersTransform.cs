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

        _upT.localRotation = _targetUpT.localRotation;// Quaternion.Lerp(_upT.localRotation, _targetUpT.localRotation, 6f * Time.deltaTime);
    }
}
