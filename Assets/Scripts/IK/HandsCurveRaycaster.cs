using Unity.VisualScripting;
using UnityEngine;

public class HandsCurveRaycaster : MonoBehaviour
{
    [SerializeField] private Transform _modelT;
    [SerializeField] private Transform _rightHandIKTarget;
    [SerializeField] private Transform _leftHandIKTarget;
    [SerializeField] private bool _active;
    
    [SerializeField] Vector2 armLenghtCoef = new Vector2(1, 1);
    [SerializeField, Range(0, 360)] float arcAngle = 180;
    [SerializeField] private float _angle;
    [SerializeField] int armCount = 1;
    [SerializeField] float armLenght = 2f;
    [SerializeField] int armPoints = 4;
    [SerializeField] int arcResolution = 4;
    [SerializeField] LayerMask arcLayer;
    [SerializeField] bool gizmoDrawPoint = true;
    [SerializeField] bool _drawArcGizmo = true;

    public void Enable()
    {
        _active = true;
    }
    
    public void Disable()
    {
        _active = false;
    }

    private void Scan(int sign, bool gizmo)
    {
        if (_modelT)
        {
            transform.up = _modelT.up;
            transform.forward = _modelT.forward;
        }
        
        float rad = _angle * Mathf.Deg2Rad;
        float arcRadius = armLenght / armPoints;
        arcRadius *= Mathf.Sqrt(Mathf.Pow(Mathf.Cos(rad), 2) * armLenghtCoef.y +
                                Mathf.Pow(Mathf.Sin(rad), 2) * armLenghtCoef.x);
        
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation * Quaternion.Euler(0, sign * _angle, 0);
        PhysicsExtension.ArcCast(pos, rot, arcAngle, arcRadius, arcResolution, arcLayer, out RaycastHit hit,
            gizmo && _drawArcGizmo);
        
        pos = hit.point;
        rot.MatchUp(hit.normal);

        if (gizmo && gizmoDrawPoint)
            Gizmos.DrawSphere(pos, 0.1f);

        if (sign > 0)
        {
            _rightHandIKTarget.position = pos;
            _rightHandIKTarget.rotation = rot;
        }
        else
        {
            _leftHandIKTarget.position = pos;
            _leftHandIKTarget.rotation = rot;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!_active)
        {
            return;
        }
        
        Scan(1, true);
        Scan(-1, true);
    }

    private void FixedUpdate()
    {
        if (!_active)
        {
            return;
        }
        
        Scan(1, false);
        Scan(-1, false);
    }
    // [SerializeField] private List<Transform> _handsIKTargets;
    // [SerializeField] private List<Transform> _handsBones;
    // [SerializeField] private bool _active;
    //
    // [SerializeField] Vector2 armLenghtCoef = new Vector2(1, 1);
    // [SerializeField, Range(0, 360)] float arcAngle = 180;
    // [SerializeField] private float _angle;
    // [SerializeField] int armCount = 1;
    // [SerializeField] float armLenght = 2f;
    // [SerializeField] int armPoints = 4;
    // [SerializeField] int arcResolution = 4;
    // [SerializeField] LayerMask arcLayer;
    // [SerializeField] bool gizmoDrawPoint = true;
    // [SerializeField] bool _drawArcGizmo = true;
    //
    // [SerializeField] private float _distanceTreshold = 1f;
    // [SerializeField] private bool _distanceCheck;
    // [SerializeField] private float _sqrMagnitude = 1f;
    //
    // private void Scan()
    // {
    //     float rad = _angle * Mathf.Deg2Rad;
    //     float arcRadius = armLenght / armPoints;
    //     arcRadius *= Mathf.Sqrt(Mathf.Pow(Mathf.Cos(rad), 2) * armLenghtCoef.y +
    //                             Mathf.Pow(Mathf.Sin(rad), 2) * armLenghtCoef.x);
    //     
    //     Vector3 pos = transform.position;
    //     Quaternion rot = transform.rotation * Quaternion.Euler(0, _angle, 0);
    //     PhysicsExtension.ArcCast(pos, rot, arcAngle, arcRadius, arcResolution, arcLayer, out RaycastHit hit,
    //         _drawArcGizmo);
    //     
    //     pos = hit.point;
    //     rot.MatchUp(hit.normal);
    //
    //     if (gizmoDrawPoint)
    //         Gizmos.DrawSphere(pos, 0.1f);
    //
    //     _handsIKTargets[0].position = pos;
    //     _handsIKTargets[0].rotation = rot;
    //
    //     _sqrMagnitude = (_handsBones[0].position - pos).sqrMagnitude;
    //     _distanceCheck = _sqrMagnitude > _distanceTreshold;
    //
    // }
    //
    // private void OnDrawGizmos()
    // {
    //     if (!_active)
    //     {
    //         return;
    //     }
    //     
    //     Scan();
    // }
    //
    // private void FixedUpdate()
    // {
    //     if (!_active)
    //     {
    //         return;
    //     }
    //
    //     return;
    //     var points = _scan.Points();
    //
    //     if (_handsIKTargets.Count != points.Count)
    //     {
    //         Debug.LogWarning("[HandsCurveRaycaster] _handsIKTargets.Count != points.Count");
    //         return;
    //     }
    //
    //     for (int i = 0; i < points.Count; i++)
    //     {
    //         _handsIKTargets[i].position = points[i].pos;
    //         _handsIKTargets[i].rotation = points[i].rot;
    //     }
    // }
}
