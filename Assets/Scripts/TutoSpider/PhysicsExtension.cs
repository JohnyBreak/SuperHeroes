using UnityEngine;

public static class PhysicsExtension
{
    static public bool ArcCast(
        Vector3 center, 
        Quaternion rotation, 
        float angle, 
        float radius,
        int resolution,
        LayerMask layer,
        out RaycastHit hit, 
        bool drawGizmo = false)
    {
        rotation *= Quaternion.Euler(-angle/2, 0, 0);

        float dAngle = angle / resolution;
        Vector3 forwardRadius = Vector3.forward * radius;

        Vector3 A, B, AB;
        A = forwardRadius;
        B = Quaternion.Euler(dAngle, 0, 0) * forwardRadius;
        AB = B - A;
        float AB_magnitude = AB.magnitude * 1.001f;

        for (int i = 0; i < resolution; i++)
        {
            A = center + rotation * forwardRadius;
            rotation *= Quaternion.Euler(dAngle, 0, 0);
            B = center + rotation * forwardRadius;
            AB = B - A;

            if (Physics.Raycast(A, AB, out hit, AB_magnitude, layer))
            {
                if (drawGizmo)
                    Gizmos.DrawLine(A, hit.point);

                return true;
            }

            if (drawGizmo)
                Gizmos.DrawLine(A, B);
        }

        hit = new RaycastHit();
        return false;
    }
}