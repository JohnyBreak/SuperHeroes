#ifndef SELECTIVE_GRAYSCALE_INCLUDED
#define SELECTIVE_GRAYSCALE_INCLUDED
#define MAX_KEEP_COLORS 16
float4 _KeepColors[MAX_KEEP_COLORS]; // rgb = эталон, a = ширина hue
float  _KeepColorCount;
float  _MinSat;
float  _MinVal;
float  _Softness;
float  _EffectAmount;
float SG_Luminance(float3 c)
{
    return dot(c, float3(0.2126, 0.7152, 0.0722));
}
float SG_RgbToHue(float3 c)
{
    float cMax = max(c.r, max(c.g, c.b));
    float cMin = min(c.r, min(c.g, c.b));
    float d = cMax - cMin;
    if (d < 1e-5)
        return 0.0;
    float h;
    if (cMax == c.r)
        h = (c.g - c.b) / d + (c.g < c.b ? 6.0 : 0.0);
    else if (cMax == c.g)
        h = (c.b - c.r) / d + 2.0;
    else
        h = (c.r - c.g) / d + 4.0;
    return h / 6.0;
}
float SG_HueDist(float a, float b)
{
    float d = abs(a - b);
    return min(d, 1.0 - d);
}
float SG_KeepMask(float3 col)
{
    float cMax = max(col.r, max(col.g, col.b));
    float cMin = min(col.r, min(col.g, col.b));
    float sat = cMax > 1e-5 ? (cMax - cMin) / cMax : 0.0;
    if (sat < _MinSat || cMax < _MinVal)
        return 0.0;
    float hue = SG_RgbToHue(col);
    float keep = 0.0;
    int count = (int)clamp(_KeepColorCount, 0, MAX_KEEP_COLORS);
    [loop]
    for (int i = 0; i < MAX_KEEP_COLORS; i++)
    {
        if (i >= count)
            break;
        float3 refCol = _KeepColors[i].rgb;
        float width = max(_KeepColors[i].a, 0.001);
        float refHue = SG_RgbToHue(refCol);
        float d = SG_HueDist(hue, refHue);
        float inner = width * (1.0 - saturate(_Softness));
        float w = 1.0 - smoothstep(inner, width, d);
        keep = max(keep, w);
    }
    return saturate(keep);
}
float3 ApplySelectiveGrayscale(float3 col)
{
    float gray = SG_Luminance(col);
    float keep = SG_KeepMask(col);
    float3 selective = lerp(gray.xxx, col, keep);
    return lerp(col, selective, saturate(_EffectAmount));
}
#endif