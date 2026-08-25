Shader "Wolverine/MeatSolid"
{
    Properties
    {
        _Color ("Color", Color) = (0.55, 0.12, 0.12, 1)
        _BoneColor ("Bone Color", Color) = (0.9, 0.88, 0.78, 1)
        _BoneBand ("Bone Band", Range(0, 1)) = 0.15
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" }
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "UnityCG.cginc"

            float4 _Color;
            float4 _BoneColor;
            float _BoneBand;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float2 uv : TEXCOORD1;
            };

            VertexOutput VertexProgram(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldNormal = UnityObjectToWorldNormal(input.normal);
                output.uv = input.uv;
                return output;
            }

            float4 FragmentProgram(VertexOutput input) : SV_Target
            {
                float lighting = saturate(dot(normalize(input.worldNormal), normalize(float3(0.35, 0.9, 0.25))));
                float boneMask = smoothstep(0.5 - _BoneBand, 0.5 + _BoneBand, input.uv.y);
                float3 albedo = lerp(_Color.rgb, _BoneColor.rgb, boneMask * 0.35);
                albedo *= 0.4 + 0.6 * lighting;
                return float4(albedo, 1.0);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
