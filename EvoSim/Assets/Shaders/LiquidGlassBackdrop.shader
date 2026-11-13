// Real-time Apple Liquid Glass backdrop: rounded SDF lens, 13-tap frosted blur, curved bevel refraction & subtle dispersion.
Shader "Evo/LiquidGlassBackdrop"
{
    Properties
    {
        _BackdropTex ("Backdrop", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent+50" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BackdropTex);
            SAMPLER(sampler_BackdropTex);
            float4 _BackdropTex_TexelSize;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 meta : TEXCOORD1;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 meta : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.screenPos = ComputeScreenPos(o.positionCS);
                o.color = input.color;
                o.uv = input.uv;
                o.meta = input.meta;
                return o;
            }

            half3 SampleFrost(float2 uv, float blurScale)
            {
                float2 texel = _BackdropTex_TexelSize.xy * blurScale;
                half3 sum = SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv).rgb * 0.20h;
                const float2 d0 = float2(1.0, 0.0);
                const float2 d1 = float2(0.5, 0.8660254);
                const float2 d2 = float2(-0.5, 0.8660254);
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + d0 * texel * 1.45).rgb * 0.09h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - d0 * texel * 1.45).rgb * 0.09h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + d1 * texel * 1.45).rgb * 0.09h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - d1 * texel * 1.45).rgb * 0.09h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + d2 * texel * 1.45).rgb * 0.09h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - d2 * texel * 1.45).rgb * 0.09h;

                const float2 o0 = float2(0.8660254, 0.5);
                const float2 o1 = float2(0.0, 1.0);
                const float2 o2 = float2(-0.8660254, 0.5);
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + o0 * texel * 3.1).rgb * 0.04333h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - o0 * texel * 3.1).rgb * 0.04333h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + o1 * texel * 3.1).rgb * 0.04333h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - o1 * texel * 3.1).rgb * 0.04333h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv + o2 * texel * 3.1).rgb * 0.04333h;
                sum += SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, uv - o2 * texel * 3.1).rgb * 0.04333h;
                return sum;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 sizePx = max(input.meta.xy, float2(2.0, 2.0));
                float radiusPx = clamp(input.meta.z, 0.0, 0.5 * min(sizePx.x, sizePx.y));
                float2 halfSize = 0.5 * sizePx;
                float2 p = (input.uv - 0.5) * sizePx;
                float2 q = abs(p) - (halfSize - radiusPx);
                float dist = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radiusPx;
                float mask = saturate(0.65 - dist);
                if (mask <= 0.002) discard;

                float2 grad = (q.x > 0.0 && q.y > 0.0) ? normalize(max(q, 1e-4)) : ((q.x > q.y) ? float2(1.0, 0.0) : float2(0.0, 1.0));
                grad *= sign(p);

                float bevel = saturate(1.0 + dist / 18.0);
                float bevelCurve = bevel * bevel;
                float refractAmt = input.meta.w;

                float2 suv = input.screenPos.xy / max(1e-5, input.screenPos.w);
                float2 lensShift = -grad * (bevelCurve * 0.015 * refractAmt);
                float blurWeight = lerp(1.0, 0.35, bevelCurve * 0.65);

                half3 blurred = SampleFrost(clamp(suv + lensShift, 0.002, 0.998), blurWeight);
                half rEdge = SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, clamp(suv + lensShift * 1.28, 0.002, 0.998)).r;
                half bEdge = SAMPLE_TEXTURE2D(_BackdropTex, sampler_BackdropTex, clamp(suv + lensShift * 0.72, 0.002, 0.998)).b;
                blurred.r = lerp(blurred.r, rEdge, bevelCurve * 0.28h);
                blurred.b = lerp(blurred.b, bEdge, bevelCurve * 0.28h);

                // Boost vibrancy slightly like Apple optical material
                half luma = dot(blurred, half3(0.2126h, 0.7152h, 0.0722h));
                blurred = max(half3(0, 0, 0), lerp(half3(luma, luma, luma), blurred, 1.32h) * 1.15h);

                // Composite pearlescent frosted glass body over blurred/refracted world
                half3 glassTint = input.color.rgb;
                half tintAlpha = input.color.a;
                half3 col = lerp(blurred, glassTint, tintAlpha);
                col += half3(0.76h, 0.91h, 1.0h) * 0.042h;

                // Subtle upper-left specular liquid sheen & curved meniscus glow
                float topGlow = saturate((input.uv.y - 0.50) / 0.50) * saturate(1.15 - input.uv.x * 0.50);
                col += half3(0.86h, 0.96h, 1.0h) * (topGlow * 0.065h);
                float lightDir = dot(normalize(float2(-0.65, 0.76)), grad);
                float rimLight = bevelCurve * bevel * (0.11 + 0.11 * lightDir);
                col += lerp(half3(0.35h, 0.92h, 0.82h), half3(1.0h, 1.0h, 1.0h), saturate(lightDir * 0.5 + 0.5)) * rimLight;

                return half4(saturate(col), mask);
            }
            ENDHLSL
        }
    }
}
