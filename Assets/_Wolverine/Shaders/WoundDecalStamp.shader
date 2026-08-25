Shader "Wolverine/WoundDecalStamp"
{
    Properties
    {
        _StampIntensity ("Stamp Intensity", Range(0, 1)) = 1
        _StampSoftness ("Stamp Softness", Range(0.01, 1)) = 0.35
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" }
        ZWrite Off
        ZTest Always
        Cull Off
        BlendOp Max
        Blend One One

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "UnityCG.cginc"

            float4x4 _ProjectorMatrix;
            float _StampIntensity;
            float _StampSoftness;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
            };

            VertexOutput VertexProgram(VertexInput input)
            {
                VertexOutput output;

                // Rasterize into mesh UV space so the stamp lands in the wound mask atlas.
                float2 uv = input.uv;
                float2 clipXy = uv * 2.0 - 1.0;
                #if UNITY_UV_STARTS_AT_TOP
                clipXy.y *= -1.0;
                #endif
                output.position = float4(clipXy, 0.0, 1.0);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                return output;
            }

            float4 FragmentProgram(VertexOutput input) : SV_Target
            {
                float3 projectorSpace = mul(_ProjectorMatrix, float4(input.worldPosition, 1.0)).xyz;
                float radialDistance = length(projectorSpace.xy);

                if (radialDistance > 1.0 || projectorSpace.z < 0.0 || projectorSpace.z > 1.0)
                {
                    discard;
                }

                float softStart = saturate(1.0 - _StampSoftness);
                float radialFalloff = 1.0 - smoothstep(softStart, 1.0, radialDistance);
                float depthFalloff = 1.0 - projectorSpace.z;
                float stamp = radialFalloff * depthFalloff * _StampIntensity;
                return float4(stamp, stamp, stamp, 1.0);
            }
            ENDCG
        }
    }

    FallBack Off
}
