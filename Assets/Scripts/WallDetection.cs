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
        if (Physics.Raycast(transform.position, _model.forward, out var hit, 1, _mask)) 
        {
            //_model.up = hit.normal;
            //transform.forward = -hit.normal;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            //var forward = transform.TransformDirection(new Vector3(0, 0, 1));

            //Quaternion lookRotation = Quaternion.LookRotation(forward, hit.normal);

            //_model.localRotation = Quaternion.LookRotation(forward, hit.normal);//Quaternion.Lerp(transform.rotation, lookRotation, 7 * Time.deltaTime);

            transform.position = hit.point + transform.TransformDirection(new Vector3(0, 1f, 0));


            ToggleWallMovement(true);
        }
    }


    //private void Update()
    //{
    //    Debug.DrawRay(transform.position, _model.forward, Color.black);
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
    }
}
