Shader "Custom/URP_AcidBubbleMesh_SolidColor"
{
    Properties
    {
        [HDR]_BubbleColor("Bubble Color", Color) = (0.65, 1.4, 0.05, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.45
        _Fade("Fade", Range(0, 1)) = 1
        _EmissionIntensity("Emission Intensity", Range(0, 5)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "SolidAcidBubble"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BubbleColor;
                float _Alpha;
                float _Fade;
                float _EmissionIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 color = _BubbleColor.rgb * _EmissionIntensity;
                float alpha = _BubbleColor.a * _Alpha * _Fade;

                return half4(color, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}