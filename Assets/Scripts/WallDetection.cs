using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private Transform _model;

    [SerializeField] private MonoBehaviour _simpleMovement;
    [SerializeField] private MonoBehaviour _wallMovement;

    private bool _onWall => _wallMovement.enabled;

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
        var upDir = transform.TransformDirection(new Vector3(0, 1, 0));
        if (Physics.Raycast(transform.position + upDir, transform.forward, out var hit, 1, _mask)) 
        {
            //_model.up = hit.normal;
            transform.forward = -hit.normal;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            //var forward = transform.TransformDirection(new Vector3(0, 0, 1));

            //Quaternion lookRotation = Quaternion.LookRotation(forward, hit.normal);

            //_model.localRotation = Quaternion.LookRotation(forward, hit.normal);//Quaternion.Lerp(transform.rotation, lookRotation, 7 * Time.deltaTime);

            _model.position = hit.point + transform.TransformDirection(new Vector3(0, 0.1f, 0));

            _simpleMovement.enabled = false;
            _wallMovement.enabled = true;
        }
    }


    //private void Update()
    //{
    //    if (_onWall)
    //    {
    //        var dir = transform.TransformDirection(new Vector3(0, -1, 0));
    //        if (Physics.Raycast(transform.position, dir, 1, _mask) == false)
    //        {
    //            ReleaseWall();
    //        }
    //    }
    //}

    private void ReleaseWall()
    {
        if (_wallMovement.enabled)
        {
            _model.up = Vector3.up;
            _simpleMovement.enabled = true;
            _wallMovement.enabled = false;
        }
    }
}
