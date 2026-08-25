Shader "Wolverine/WoundMaskFade"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "white" {}
        _FadeAmount ("Fade Amount", Range(0, 1)) = 0.02
    }

    SubShader
    {
        Tags { "Queue" = "Overlay" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex VertexProgram
            #pragma fragment FragmentProgram
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _FadeAmount;

            struct VertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            VertexOutput VertexProgram(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            float4 FragmentProgram(VertexOutput input) : SV_Target
            {
                float mask = tex2D(_MainTex, input.uv).r;
                float healed = saturate(mask - _FadeAmount);
                return float4(healed, healed, healed, 1.0);
            }
            ENDCG
        }
    }

    FallBack Off
}
