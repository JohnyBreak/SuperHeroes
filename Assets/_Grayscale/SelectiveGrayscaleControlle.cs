using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class SelectiveGrayscaleController : MonoBehaviour
{
    const int MaxColors = 16;

    [Header("Toggle")]
    public bool effectEnabled = true;

    [Header("Setup")]
    public Material effectMaterial;

    [Tooltip("Renderer с Full Screen Pass. Если задан — pass полностью отключается (без лишнего blit).")]
    public UniversalRendererData rendererData;

    [Header("Keep Colors")]
    [Range(0f, 1f)] public float minSaturation = 0.25f;
    [Range(0f, 1f)] public float minValue = 0.1f;
    [Range(0f, 1f)] public float softness = 0.5f;

    [Tooltip("RGB = цвет сохранить, A = ширина hue (0.03–0.12)")]
    public Color[] keepColors =
    {
        new Color(0.85f, 0.05f, 0.05f, 0.08f),
        new Color(0.10f, 0.80f, 0.15f, 0.08f),
        new Color(0.15f, 0.25f, 0.90f, 0.08f),
    };

    static readonly int KeepColorsId = Shader.PropertyToID("_KeepColors");
    static readonly int KeepCountId = Shader.PropertyToID("_KeepColorCount");
    static readonly int MinSatId = Shader.PropertyToID("_MinSat");
    static readonly int MinValId = Shader.PropertyToID("_MinVal");
    static readonly int SoftnessId = Shader.PropertyToID("_Softness");
    static readonly int EffectAmountId = Shader.PropertyToID("_EffectAmount");

    readonly Vector4[] _buffer = new Vector4[MaxColors];
    FullScreenPassRendererFeature _fullScreenPass;

    void OnEnable() => Push();
    void OnValidate() => Push();
    void LateUpdate() => Push();

    public void SetEffectEnabled(bool enabled)
    {
        effectEnabled = enabled;
        Push();
    }

    void Push()
    {
        ApplyFeatureActive();

        if (effectMaterial == null)
            return;

        int count = Mathf.Min(keepColors != null ? keepColors.Length : 0, MaxColors);

        for (int i = 0; i < count; i++)
        {
            Color c = keepColors[i];
            _buffer[i] = new Vector4(c.r, c.g, c.b, c.a > 0f ? c.a : 0.08f);
        }

        for (int i = count; i < MaxColors; i++)
            _buffer[i] = Vector4.zero;

        effectMaterial.SetVectorArray(KeepColorsId, _buffer);
        effectMaterial.SetFloat(KeepCountId, count);
        effectMaterial.SetFloat(MinSatId, minSaturation);
        effectMaterial.SetFloat(MinValId, minValue);
        effectMaterial.SetFloat(SoftnessId, softness);
        effectMaterial.SetFloat(EffectAmountId, effectEnabled ? 1f : 0f);
    }

    void ApplyFeatureActive()
    {
        var feature = ResolveFullScreenPass();
        if (feature == null)
            return;

        if (feature.isActive != effectEnabled)
            feature.SetActive(effectEnabled);
    }

    FullScreenPassRendererFeature ResolveFullScreenPass()
    {
        if (_fullScreenPass != null)
            return _fullScreenPass;

        if (rendererData == null)
            return null;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is not FullScreenPassRendererFeature fullScreen)
                continue;

            if (effectMaterial != null && fullScreen.passMaterial != effectMaterial)
                continue;

            _fullScreenPass = fullScreen;
            return _fullScreenPass;
        }

        return null;
    }
}
