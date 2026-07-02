Shader "Custom/URP_AcidSurface_TwoLayerCaustic"
{
    Properties
    {
        [Header(Top Layer Color)]
        [HDR]_DarkColor("Top Dark Acid Color", Color) = (0.08, 0.38, 0.02, 1)
        [HDR]_BaseColor("Top Base Acid Color", Color) = (0.45, 1.0, 0.02, 1)
        [HDR]_BrightColor("Top Bright Acid Color", Color) = (1.4, 2.5, 0.05, 1)
        _TopLayerOpacity("Top Layer Opacity", Range(0, 1)) = 0.72

        [Header(Bottom Layer Color)]
        [HDR]_BottomDarkColor("Bottom Dark Color", Color) = (0.03, 0.20, 0.02, 1)
        [HDR]_BottomBaseColor("Bottom Base Color", Color) = (0.18, 0.75, 0.02, 1)
        [HDR]_BottomBrightColor("Bottom Bright Color", Color) = (0.8, 1.8, 0.05, 1)
        _BottomLayerIntensity("Bottom Layer Intensity", Range(0, 3)) = 1.0

        [Header(Noise Textures)]
        _NoiseTex("Main Large Noise", 2D) = "white" {}
        _DistortTex("Distort Noise", 2D) = "white" {}
        _CausticTex("Caustic Texture", 2D) = "white" {}

        [Header(Top Layer Flow)]
        _NoiseTiling("Top Noise Tiling", Float) = 1.0
        _NoiseSpeed("Top Noise Speed", Float) = 0.08
        _NoiseStrength("Noise Strength > 1", Range(0, 3)) = 1.8
        _DistortTiling("Top Distort Tiling", Float) = 2.0
        _DistortSpeed("Top Distort Speed", Float) = 0.12
        _DistortStrength("Top Distort Strength", Range(0, 0.5)) = 0.08

        [Header(Bottom Layer Flow)]
        _BottomNoiseTiling("Bottom Noise Tiling", Float) = 0.65
        _BottomNoiseSpeed("Bottom Noise Speed", Float) = 0.035
        _BottomDistortStrength("Bottom Distort Strength", Range(0, 0.5)) = 0.05

        [Header(Vertex Wave)]
        _VertexWaveTiling("Vertex Wave Tiling", Float) = 1.2
        _VertexWaveSpeed("Vertex Wave Speed", Float) = 0.7
        _VertexWaveStrength("Vertex Wave Strength", Range(0, 1)) = 0.08
        _VertexNoiseStrength("Vertex Noise Strength", Range(0, 1)) = 0.06

        [Header(Stylized Color Bands)]
        _ColorBands("Color Bands", Range(2, 8)) = 4
        _BandContrast("Band Contrast", Range(0.5, 3)) = 1.5

        [Header(Sparse Acid Lines)]
        [HDR]_LineColor("Acid Line Color", Color) = (2.0, 3.2, 0.05, 1)
        _LineThreshold("Line Threshold", Range(0, 1)) = 0.72
        _LineWidth("Line Width", Range(0.005, 0.3)) = 0.06
        _LineSoftness("Line Softness", Range(0.001, 0.3)) = 0.04
        _LineIntensity("Line Intensity", Range(0, 5)) = 1.1
        _LineNoiseBreak("Line Break Noise", Range(0, 1)) = 0.4

        [Header(Caustics)]
        [HDR]_CausticColor("Caustic Color", Color) = (1.6, 2.8, 0.08, 1)
        _CausticTiling("Caustic Tiling", Float) = 2.0
        _CausticSpeed("Caustic Speed", Float) = 0.25
        _CausticDistort("Caustic Distort", Range(0, 0.5)) = 0.08
        _CausticThreshold("Caustic Threshold", Range(0, 1)) = 0.55
        _CausticSoftness("Caustic Softness", Range(0.001, 0.5)) = 0.12
        _CausticIntensity("Caustic Intensity", Range(0, 6)) = 1.6

        [Header(Contact Ripple)]
        [HDR]_ContactColor("Contact Ripple Color", Color) = (2.4, 3.8, 0.08, 1)
        _ContactWidth("Contact Width", Range(0.01, 3)) = 0.35
        _RippleDistance("Ripple Distance", Range(0.05, 5)) = 1.4
        _RippleFrequency("Ripple Frequency", Range(1, 40)) = 12
        _RippleSpeed("Ripple Speed", Range(0, 10)) = 1.2
        _RippleLineWidth("Ripple Line Width", Range(0.01, 0.8)) = 0.18
        _RippleIntensity("Ripple Intensity", Range(0, 6)) = 2.2
        _RippleNoiseStrength("Ripple Noise Strength", Range(0, 5)) = 2.0

        [Header(Fresnel)]
        [HDR]_FresnelColor("Fresnel Color", Color) = (1.2, 2.2, 0.05, 1)
        _FresnelPower("Fresnel Power", Range(0.2, 8)) = 3.0
        _FresnelIntensity("Fresnel Intensity", Range(0, 5)) = 0.35

        [Header(Final)]
        _Alpha("Alpha", Range(0, 1)) = 0.78
        _EmissionIntensity("Emission Intensity", Range(0, 8)) = 1.8
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
            Name "AcidSurface"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
            };

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            TEXTURE2D(_DistortTex);
            SAMPLER(sampler_DistortTex);

            TEXTURE2D(_CausticTex);
            SAMPLER(sampler_CausticTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _DarkColor;
                float4 _BaseColor;
                float4 _BrightColor;
                float _TopLayerOpacity;

                float4 _BottomDarkColor;
                float4 _BottomBaseColor;
                float4 _BottomBrightColor;
                float _BottomLayerIntensity;

                float _NoiseTiling;
                float _NoiseSpeed;
                float _NoiseStrength;
                float _DistortTiling;
                float _DistortSpeed;
                float _DistortStrength;

                float _BottomNoiseTiling;
                float _BottomNoiseSpeed;
                float _BottomDistortStrength;

                float _VertexWaveTiling;
                float _VertexWaveSpeed;
                float _VertexWaveStrength;
                float _VertexNoiseStrength;

                float _ColorBands;
                float _BandContrast;

                float4 _LineColor;
                float _LineThreshold;
                float _LineWidth;
                float _LineSoftness;
                float _LineIntensity;
                float _LineNoiseBreak;

                float4 _CausticColor;
                float _CausticTiling;
                float _CausticSpeed;
                float _CausticDistort;
                float _CausticThreshold;
                float _CausticSoftness;
                float _CausticIntensity;

                float4 _ContactColor;
                float _ContactWidth;
                float _RippleDistance;
                float _RippleFrequency;
                float _RippleSpeed;
                float _RippleLineWidth;
                float _RippleIntensity;
                float _RippleNoiseStrength;

                float4 _FresnelColor;
                float _FresnelPower;
                float _FresnelIntensity;

                float _Alpha;
                float _EmissionIntensity;

            CBUFFER_END

            float Posterize01(float v, float bands)
            {
                bands = max(bands, 2.0);
                return floor(saturate(v) * bands) / bands;
            }

            float3 ThreeColorLerp(float t, float3 darkCol, float3 midCol, float3 brightCol)
            {
                if (t < 0.5)
                {
                    float k = t / 0.5;
                    return lerp(darkCol, midCol, k);
                }
                else
                {
                    float k = (t - 0.5) / 0.5;
                    return lerp(midCol, brightCol, k);
                }
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float time = _Time.y;
                float2 uv = IN.uv;

                float3 posOS = IN.positionOS.xyz;

                // 顶点起伏：不是波点，是整片面片上下起伏
                float waveA = sin((posOS.x + posOS.z) * _VertexWaveTiling + time * _VertexWaveSpeed);
                float waveB = sin((posOS.x - posOS.z) * (_VertexWaveTiling * 0.73) - time * _VertexWaveSpeed * 0.8);

                float2 vertexNoiseUV = uv * (_NoiseTiling * 0.75) + float2(time * _NoiseSpeed, -time * _NoiseSpeed * 0.5);
                float vertexNoise = SAMPLE_TEXTURE2D_LOD(_NoiseTex, sampler_NoiseTex, vertexNoiseUV, 0).r;

                float wave = (waveA + waveB) * 0.5;
                float noiseWave = (vertexNoise - 0.5) * 2.0;

                float heightOffset = wave * _VertexWaveStrength + noiseWave * _VertexNoiseStrength;

                posOS += IN.normalOS * heightOffset;

                VertexPositionInputs posInput = GetVertexPositionInputs(posOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInput.positionCS;
                OUT.positionWS = posInput.positionWS;
                OUT.normalWS = normalize(normalInput.normalWS);
                OUT.uv = uv;
                OUT.screenPos = ComputeScreenPos(posInput.positionCS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
                float2 uv = IN.uv;

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - IN.positionWS);

                // -------------------------
                // 1. 上层酸液扰动
                // -------------------------

                float2 distortUV1 = uv * _DistortTiling + float2(time * _DistortSpeed, -time * _DistortSpeed * 0.5);
                float2 distortUV2 = uv * _DistortTiling * 1.37 + float2(-time * _DistortSpeed * 0.4, time * _DistortSpeed);

                float2 topDistort;
                topDistort.x = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, distortUV1).r;
                topDistort.y = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, distortUV2).g;
                topDistort = (topDistort - 0.5) * _DistortStrength;

                float2 topNoiseUV1 = uv * _NoiseTiling + topDistort + float2(time * _NoiseSpeed, time * _NoiseSpeed * 0.25);
                float2 topNoiseUV2 = uv * _NoiseTiling * 1.21 - topDistort + float2(-time * _NoiseSpeed * 0.45, time * _NoiseSpeed * 0.6);

                float topN1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, topNoiseUV1).r;
                float topN2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, topNoiseUV2).g;

                float topNoise = saturate(topN1 * 0.75 + topN2 * 0.25);

                // Noise Strength 可以大于 1，让酸液大块明暗更强
                topNoise = 0.5 + (topNoise - 0.5) * _NoiseStrength;
                topNoise = saturate(topNoise);

                topNoise = saturate((topNoise - 0.5) * _BandContrast + 0.5);

                float topBand = Posterize01(topNoise, _ColorBands);
                float3 topColor = ThreeColorLerp(topBand, _DarkColor.rgb, _BaseColor.rgb, _BrightColor.rgb);

                // -------------------------
                // 2. 下层酸液
                // -------------------------

                float2 bottomDistortUV = uv * (_DistortTiling * 0.55) + float2(-time * _DistortSpeed * 0.3, time * _DistortSpeed * 0.2);
                float2 bottomDistort;
                bottomDistort.x = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, bottomDistortUV).b;
                bottomDistort.y = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, bottomDistortUV * 1.31).r;
                bottomDistort = (bottomDistort - 0.5) * _BottomDistortStrength;

                float2 bottomUV1 = uv * _BottomNoiseTiling + bottomDistort + float2(-time * _BottomNoiseSpeed, time * _BottomNoiseSpeed * 0.35);
                float2 bottomUV2 = uv * _BottomNoiseTiling * 1.17 - bottomDistort + float2(time * _BottomNoiseSpeed * 0.4, -time * _BottomNoiseSpeed * 0.25);

                float bottomN1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, bottomUV1).b;
                float bottomN2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, bottomUV2).r;

                float bottomNoise = saturate(bottomN1 * 0.65 + bottomN2 * 0.35);
                bottomNoise = saturate((bottomNoise - 0.5) * 1.25 + 0.5);

                float bottomBand = Posterize01(bottomNoise, max(2.0, _ColorBands - 1.0));
                float3 bottomColor = ThreeColorLerp(
                    bottomBand,
                    _BottomDarkColor.rgb,
                    _BottomBaseColor.rgb,
                    _BottomBrightColor.rgb
                ) * _BottomLayerIntensity;

                // -------------------------
                // 3. 焦散 Caustics，下层更明显
                // -------------------------

                float2 causticDistortUV = uv * (_DistortTiling * 0.8) + float2(time * 0.06, -time * 0.04);
                float2 causticDistort;
                causticDistort.x = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, causticDistortUV).r;
                causticDistort.y = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, causticDistortUV * 1.43).g;
                causticDistort = (causticDistort - 0.5) * _CausticDistort;

                float2 causticUV1 = uv * _CausticTiling + causticDistort + float2(time * _CausticSpeed, time * _CausticSpeed * 0.35);
                float2 causticUV2 = uv * (_CausticTiling * 1.28) - causticDistort + float2(-time * _CausticSpeed * 0.6, time * _CausticSpeed * 0.45);

                float c1 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV1).r;
                float c2 = SAMPLE_TEXTURE2D(_CausticTex, sampler_CausticTex, causticUV2).g;

                float caustic = saturate(c1 * c2 * 2.0);
                caustic = smoothstep(_CausticThreshold, _CausticThreshold + _CausticSoftness, caustic);

                bottomColor += caustic * _CausticColor.rgb * _CausticIntensity;

                // -------------------------
                // 4. 上层稀疏亮纹
                // -------------------------

                float contour = abs(topNoise - _LineThreshold);
                float lineMask = 1.0 - smoothstep(_LineWidth, _LineWidth + _LineSoftness, contour);

                float breakNoise = SAMPLE_TEXTURE2D(
                    _DistortTex,
                    sampler_DistortTex,
                    uv * (_DistortTiling * 1.5) + float2(time * 0.05, -time * 0.04)
                ).r;

                lineMask *= lerp(1.0, step(0.35, breakNoise), _LineNoiseBreak);

                topColor += lineMask * _LineColor.rgb * _LineIntensity;

                // -------------------------
                // 5. 混合上下两层
                // -------------------------

                float3 acidColor = lerp(bottomColor, topColor, _TopLayerOpacity);

                // 焦散再轻微穿透到上层，形成酸液内部发亮感
                acidColor += caustic * _CausticColor.rgb * (_CausticIntensity * 0.25);

                // -------------------------
                // 6. 静态物体接触水波
                // -------------------------

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                float rawSceneDepth = SampleSceneDepth(screenUV);
                float rawSurfaceDepth = IN.screenPos.z / IN.screenPos.w;

                float sceneEyeDepth = LinearEyeDepth(rawSceneDepth, _ZBufferParams);
                float surfaceEyeDepth = LinearEyeDepth(rawSurfaceDepth, _ZBufferParams);

                float depthDiff = abs(sceneEyeDepth - surfaceEyeDepth);

                float contactMask = 1.0 - smoothstep(0.0, _ContactWidth, depthDiff);

                float rippleArea = 1.0 - smoothstep(_ContactWidth, _RippleDistance, depthDiff);
                rippleArea *= smoothstep(0.0, _ContactWidth, depthDiff);

                float rippleNoise = SAMPLE_TEXTURE2D(
                    _DistortTex,
                    sampler_DistortTex,
                    uv * (_DistortTiling * 2.0) + float2(time * 0.06, -time * 0.04)
                ).r;

                float noiseOffset = (rippleNoise - 0.5) * _RippleNoiseStrength;

                float wave = sin((depthDiff + noiseOffset) * _RippleFrequency - time * _RippleSpeed);
                wave = wave * 0.5 + 0.5;

                float rippleLine = smoothstep(1.0 - _RippleLineWidth, 1.0, wave);
                float rippleMask = rippleLine * rippleArea;

                float contactGlow = contactMask * 0.7;
                float finalRipple = saturate(rippleMask + contactGlow);

                acidColor += finalRipple * _ContactColor.rgb * _RippleIntensity;

                // -------------------------
                // 7. Fresnel
                // -------------------------

                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                fresnel = pow(fresnel, _FresnelPower);
                acidColor += fresnel * _FresnelColor.rgb * _FresnelIntensity;

                // -------------------------
                // 8. 输出
                // -------------------------

                acidColor *= _EmissionIntensity;

                float alpha = _Alpha;
                alpha += lineMask * 0.08;
                alpha += caustic * 0.08;
                alpha += finalRipple * 0.12;
                alpha = saturate(alpha);

                return half4(acidColor, alpha);
            }

            ENDHLSL
        }
    }

    FallBack Off
}