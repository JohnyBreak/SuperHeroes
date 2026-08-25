Shader "Wolverine/SkinInteriorClip"
{
    Properties
    {
        _Color ("Interior Color", Color) = (0.45, 0.1, 0.1, 1)
        _WoundMask ("Wound Mask", 2D) = "black" {}
        _ClipThreshold ("Clip Threshold", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        // Draw the inside of the outer shell so holes are not see-through glass.
        Cull Front
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "UnityCG.cginc"

            sampler2D _WoundMask;
            float4 _Color;
            float _ClipThreshold;

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
                // Flip normal because we cull front faces (viewing the shell from inside).
                output.worldNormal = -UnityObjectToWorldNormal(input.normal);
                return output;
            }

            float4 FragmentProgram(VertexOutput input) : SV_Target
            {
                float mask = tex2D(_WoundMask, input.uv).r;
                clip(_ClipThreshold - mask);

                float lighting = saturate(dot(normalize(input.worldNormal), normalize(float3(0.35, 0.9, 0.25))));
                float3 albedo = _Color.rgb * (0.4 + 0.6 * lighting);
                return float4(albedo, 1.0);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
