Shader "Custom/URP_AcidBubblePopRing"
{
    Properties
    {
        [HDR]_RingColor("Ring Color", Color) = (2.2, 3.8, 0.08, 1)

        _Fade("Fade", Range(0, 1)) = 1
        _Alpha("Alpha", Range(0, 1)) = 0.75

        _RingRadius("Ring Radius", Range(0, 1.5)) = 0.55
        _RingWidth("Ring Width", Range(0.001, 0.5)) = 0.08
        _RingSoftness("Ring Softness", Range(0.001, 0.5)) = 0.06
        _Intensity("Intensity", Range(0, 8)) = 2.0

        _NoiseStrength("Noise Strength", Range(0, 0.5)) = 0.08
        _DistortTex("Distort Noise", 2D) = "white" {}
        _NoiseTiling("Noise Tiling", Float) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent+10"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "BubblePopRing"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_DistortTex);
            SAMPLER(sampler_DistortTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _RingColor;

                float _Fade;
                float _Alpha;

                float _RingRadius;
                float _RingWidth;
                float _RingSoftness;
                float _Intensity;

                float _NoiseStrength;
                float _NoiseTiling;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 centerUV = IN.uv - 0.5;
                float d = length(centerUV) * 2.0;

                float noise = SAMPLE_TEXTURE2D(
                    _DistortTex,
                    sampler_DistortTex,
                    IN.uv * _NoiseTiling + float2(_Time.y * 0.08, -_Time.y * 0.05)
                ).r;

                d += (noise - 0.5) * _NoiseStrength;

                float ring = 1.0 - smoothstep(
                    _RingWidth,
                    _RingWidth + _RingSoftness,
                    abs(d - _RingRadius)
                );

                float alpha = ring * _Alpha * _Fade;
                float3 color = _RingColor.rgb * ring * _Intensity;

                return half4(color, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}