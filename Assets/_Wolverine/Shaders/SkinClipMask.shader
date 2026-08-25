Shader "Wolverine/SkinClipMask"
{
    Properties
    {
        _Color ("Color", Color) = (0.82, 0.62, 0.48, 1)
        _WoundMask ("Wound Mask", 2D) = "black" {}
        _ClipThreshold ("Clip Threshold", Range(0, 1)) = 0.35
        _EdgeColor ("Edge Color", Color) = (0.45, 0.08, 0.08, 1)
        _EdgeWidth ("Edge Width", Range(0.01, 0.5)) = 0.12
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "UnityCG.cginc"

            sampler2D _WoundMask;
            float4 _Color;
            float4 _EdgeColor;
            float _ClipThreshold;
            float _EdgeWidth;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            VertexOutput VertexProgram(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                return output;
            }

            float4 FragmentProgram(VertexOutput input) : SV_Target
            {
                float mask = tex2D(_WoundMask, input.uv).r;

                // High mask = hole in the outer skin layer.
                clip(_ClipThreshold - mask);

                float edge = saturate((_ClipThreshold - mask) / max(_EdgeWidth, 1e-4));
                float lighting = saturate(dot(normalize(input.worldNormal), normalize(float3(0.35, 0.9, 0.25))));
                float3 albedo = lerp(_EdgeColor.rgb, _Color.rgb, edge);
                albedo *= 0.35 + 0.65 * lighting;
                return float4(albedo, 1.0);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
