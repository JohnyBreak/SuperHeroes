using System.Collections.Generic;
using UnityEngine;

public class ScanArmsAdvanced : Scan
{
    [SerializeField] int armCount = 11;
    [SerializeField] float armLenght = 2f;
    [SerializeField] Vector2 armLenghtCoef = new Vector2(1, 1);
    [SerializeField] int armPoints = 4;

    //[SerializeField] Player3D player3D;
    [SerializeField] float armLenghtSpeedCoef = 0;

    [SerializeField] bool weightByDist = false;

    [SerializeField, Range(0, 360)] float arcAngle = 270;
    [SerializeField] int arcResolution = 4;
    [SerializeField] LayerMask arcLayer;

    [SerializeField] bool gizmoDrawPoint = true;
    [SerializeField] bool gizmoDrawLink = true;
    [SerializeField] bool _drawArcGizmo = true;

    void OnDrawGizmosSelected()
    {
        Scan(true);
    }

    public override List<(Vector3 pos, Quaternion rot, float weight)> Points()
    {
        return Scan(false);
    }

    List<(Vector3, Quaternion, float)> Scan(bool gizmo = false)
    {
        List<(Vector3 pos, Quaternion rot, float weight)> points = new List<(Vector3, Quaternion, float)>();

        for (int i = 0; i < armCount; i++)
        {
            float angle = 360 * i / armCount;
            float rad = angle * Mathf.Deg2Rad;

            float arcRadius = armLenght / armPoints;
            arcRadius *= Mathf.Sqrt(Mathf.Pow(Mathf.Cos(rad), 2) * armLenghtCoef.y +
                                    Mathf.Pow(Mathf.Sin(rad), 2) * armLenghtCoef.x);

            //if (player3D && player3D.Velocity != Vector2.zero)
            //{
            //    float angleArmVelocity = Vector3.Angle(player3D.Velocity3, Quaternion.Euler(0, angle, 0) * Vector3.forward);
            //    float progress = Mathf.InverseLerp(90, 0, angleArmVelocity);
            //    arcRadius += progress * player3D.Speed * armLenghtSpeedCoef;
            //}

            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation * Quaternion.Euler(0, angle, 0);

            for (int j = 0; j < armPoints && PhysicsExtension.ArcCast(pos, rot, arcAngle, arcRadius, 
                     arcResolution, arcLayer, out RaycastHit hit, _drawArcGizmo); j++)
            {
                float weight = weightByDist ? 1 - (float)j / armPoints : 1;

                if (gizmo)
                    Gizmos.color = new Color(1, 1, 1, weight);

                if (gizmo && gizmoDrawLink)
                    Gizmos.DrawLine(pos, hit.point);

                pos = hit.point;
                rot.MatchUp(hit.normal);

                points.Add((pos, rot * Quaternion.Euler(0, -angle, 0), weight));

                if (gizmo && gizmoDrawPoint)
                    Gizmos.DrawSphere(pos, 0.1f);
            }
        }

        return points;
    }
}
