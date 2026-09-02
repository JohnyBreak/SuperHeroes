using UnityEngine;

public class SpineIKSolver : MonoBehaviour
{
    [SerializeField] private Transform _spineIKTarget;
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
    
    private void Scan(bool gizmo)
    {
        float rad = _angle * Mathf.Deg2Rad;
        float arcRadius = armLenght / armPoints;
        arcRadius *= Mathf.Sqrt(Mathf.Pow(Mathf.Cos(rad), 2) * armLenghtCoef.y +
                                Mathf.Pow(Mathf.Sin(rad), 2) * armLenghtCoef.x);
        
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation * Quaternion.Euler(0, _angle, 0);
        PhysicsExtension.ArcCast(pos, rot, arcAngle, arcRadius, arcResolution, arcLayer, out RaycastHit hit,
            gizmo && _drawArcGizmo);
        
        pos = hit.point;
        rot.MatchUp(hit.normal);

        if (gizmo && gizmoDrawPoint)
            Gizmos.DrawSphere(pos, 0.1f);

        _spineIKTarget.position = pos;
        _spineIKTarget.rotation = rot;
    }
    
    private void OnDrawGizmos()
    {
        if (!_active)
        {
            return;
        }
        
        Scan(true);
    }

    private void FixedUpdate()
    {
        if (!_active)
        {
            return;
        }
        
        Scan(false);
    }
}
