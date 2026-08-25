using UnityEngine;

namespace Wolverine.Wounds
{
    /// <summary>
    /// Method C: projects a local decal volume at the hit and stamps it into the UV wound mask.
    /// </summary>
    public sealed class WoundDecalPainter : MonoBehaviour
    {
        [SerializeField] private WoundMaskBuffer _woundMaskBuffer;
        [SerializeField] private MeshFilter _outerMeshFilter;
        [SerializeField] private Transform _outerTransform;
        [SerializeField] private Material _stampMaterial;
        [SerializeField] private float _stampRadius = 0.12f;
        [SerializeField] private float _stampDepth = 0.2f;
        [SerializeField] private float _stampIntensity = 1f;
        [SerializeField] private float _stampSoftness = 0.35f;

        public void Configure(
            WoundMaskBuffer woundMaskBuffer,
            MeshFilter outerMeshFilter,
            Transform outerTransform,
            Material stampMaterial)
        {
            _woundMaskBuffer = woundMaskBuffer;
            _outerMeshFilter = outerMeshFilter;
            _outerTransform = outerTransform;
            _stampMaterial = stampMaterial;
        }

        public void SetStampShape(float radius, float depth, float intensity, float softness)
        {
            _stampRadius = radius;
            _stampDepth = depth;
            _stampIntensity = intensity;
            _stampSoftness = softness;
        }

        public void StampAtHit(RaycastHit hit)
        {
            if (_woundMaskBuffer == null || _woundMaskBuffer.MaskTexture == null)
            {
                return;
            }

            if (_outerMeshFilter == null || _outerMeshFilter.sharedMesh == null || _stampMaterial == null)
            {
                return;
            }

            Matrix4x4 projectorMatrix = BuildProjectorMatrix(hit.point, hit.normal, _stampRadius, _stampDepth);
            _stampMaterial.SetMatrix("_ProjectorMatrix", projectorMatrix);
            _stampMaterial.SetFloat("_StampIntensity", _stampIntensity);
            _stampMaterial.SetFloat("_StampSoftness", _stampSoftness);

            RenderTexture previous = RenderTexture.active;
            Graphics.SetRenderTarget(_woundMaskBuffer.MaskTexture);
            _stampMaterial.SetPass(0);
            Graphics.DrawMeshNow(_outerMeshFilter.sharedMesh, _outerTransform.localToWorldMatrix);
            RenderTexture.active = previous;
        }

        private static Matrix4x4 BuildProjectorMatrix(
            Vector3 hitPoint,
            Vector3 hitNormal,
            float radius,
            float depth)
        {
            Vector3 axisZ = (-hitNormal).normalized;
            Vector3 axisX = Vector3.Cross(axisZ, Vector3.up);
            if (axisX.sqrMagnitude < 0.001f)
            {
                axisX = Vector3.Cross(axisZ, Vector3.right);
            }

            axisX.Normalize();
            Vector3 axisY = Vector3.Cross(axisZ, axisX).normalized;

            Matrix4x4 worldToProjectorBasis = Matrix4x4.identity;
            worldToProjectorBasis.SetRow(0, new Vector4(axisX.x, axisX.y, axisX.z, 0f));
            worldToProjectorBasis.SetRow(1, new Vector4(axisY.x, axisY.y, axisY.z, 0f));
            worldToProjectorBasis.SetRow(2, new Vector4(axisZ.x, axisZ.y, axisZ.z, 0f));
            worldToProjectorBasis.SetRow(3, new Vector4(0f, 0f, 0f, 1f));

            Matrix4x4 translate = Matrix4x4.Translate(-hitPoint);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(
                1f / Mathf.Max(radius, 0.0001f),
                1f / Mathf.Max(radius, 0.0001f),
                1f / Mathf.Max(depth, 0.0001f)));

            return scale * worldToProjectorBasis * translate;
        }
    }
}
