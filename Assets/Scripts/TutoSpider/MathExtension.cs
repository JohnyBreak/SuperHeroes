using System.Collections.Generic;
using UnityEngine;





public static class MathExtension
{
    public static Quaternion RotationMatchUp(Quaternion rotation, Vector3 up) => Quaternion.FromToRotation(rotation * Vector3.up, up) * rotation;
    public static void MatchUp(this ref Quaternion rotation, Vector3 up) => rotation = RotationMatchUp(rotation, up);
    public static void MatchUp(this Transform transform, Vector3 up) => transform.rotation = RotationMatchUp(transform.rotation, up);


    public static Quaternion QuatAvgApprox(Quaternion[] quats, float[] weights = null)
    {
        if (quats.Length == 0)
            return Quaternion.identity;

        if (weights != null && quats.Length != weights.Length)
            return Quaternion.identity;

        Vector4[] vects = new Vector4[quats.Length];
        for (int i = 0; i < vects.Length; i++)
            vects[i] = Quat2Vect(quats[i]);

        Vector4 vectsAvg = Vector4.zero;

        for (int i = 0; i < vects.Length; i++)
        {
            Vector4 v = vects[i];
            float w = weights == null ? 1 : weights[i];

            if (i > 0 && Vector4.Dot(v, vects[0]) < 0)
                w *= -1;

            vectsAvg += v * w;
        }

        vectsAvg.Normalize();

        return new Quaternion(vectsAvg.x, vectsAvg.y, vectsAvg.z, vectsAvg.w);
    }

    public static Vector4 Quat2Vect(Quaternion q) => new Vector4(q.x, q.y, q.z, q.w);
}
