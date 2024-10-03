using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpersTransform : MonoBehaviour
{
    [SerializeField] private Transform _upT;
    [SerializeField] private Transform _targetUpT;

    [SerializeField] private Transform _movingT;
    [SerializeField] private Transform _targetT;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _movingT.position = _targetT.position;

        _upT.localRotation = _targetUpT.localRotation;

        //_upT.forward = _targetUpT.forward;
        //_upT.up = _targetUpT.up;
    }
}
