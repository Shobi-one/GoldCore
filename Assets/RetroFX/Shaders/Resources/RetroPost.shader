// Image effect for RetroCamera: fog, blur, bloom, color grading, color depth with ordered
// dithering, and scanlines. It runs once, while writing the low resolution target, so the
// dither pattern and scanlines line up with the big retro pixels.
Shader "Hidden/GoldCore/RetroPost"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            float4 _RetroTargetSize;     // width, height, 1/width, 1/height of the output
            float _RetroColorSteps;      // 2^bits - 1
            float _RetroDither;
            float _RetroWarmShadows;
            float _RetroSaturation;
            float _RetroBlur;
            float _RetroBloom;
            float _RetroBloomThreshold;
            float4 _RetroFogColor;
            float4 _RetroFogRange;       // enabled, start, end
            float _RetroScanlines;

            // 4x4 Bayer matrix, values 0..15.
            static const float BayerMatrix[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

            float Bayer4(uint2 p)
            {
                return BayerMatrix[(p.y & 3u) * 4u + (p.x & 3u)];
            }

            float3 Bright(float2 uv)
            {
                float3 c = tex2D(_MainTex, uv).rgb;
                float l = max(c.r, max(c.g, c.b));
                return c * saturate((l - _RetroBloomThreshold) / max(1e-4, 1 - _RetroBloomThreshold));
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 px = _RetroTargetSize.zw;   // one output pixel in uv
                float3 col = tex2D(_MainTex, uv).rgb;

                if (_RetroBlur > 0)
                {
                    float2 o = px * 0.75;
                    float3 b = tex2D(_MainTex, uv + float2(o.x, o.y)).rgb
                             + tex2D(_MainTex, uv + float2(-o.x, o.y)).rgb
                             + tex2D(_MainTex, uv + float2(o.x, -o.y)).rgb
                             + tex2D(_MainTex, uv + float2(-o.x, -o.y)).rgb;
                    col = lerp(col, b * 0.25, _RetroBlur);
                }

                if (_RetroFogRange.x > 0)
                {
                    float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                    float dist = LinearEyeDepth(rawDepth);
                    float f = saturate((dist - _RetroFogRange.y) / (_RetroFogRange.z - _RetroFogRange.y));
                    col = lerp(col, _RetroFogColor.rgb, f * _RetroFogColor.a);
                }

                if (_RetroBloom > 0)
                {
                    float2 r = px * 3;
                    float3 g = Bright(uv + float2(r.x, 0)) + Bright(uv - float2(r.x, 0))
                             + Bright(uv + float2(0, r.y)) + Bright(uv - float2(0, r.y))
                             + Bright(uv + r * 0.7) + Bright(uv - r * 0.7)
                             + Bright(uv + float2(r.x, -r.y) * 0.7) + Bright(uv + float2(-r.x, r.y) * 0.7);
                    col += g * (0.125 * _RetroBloom);
                }

                float luma = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(luma.xxx, col, _RetroSaturation);
                if (_RetroWarmShadows > 0)
                {
                    float dark = 1 - saturate(luma * 2);
                    col *= lerp(float3(1, 1, 1), float3(1.0, 0.82, 0.8), dark * _RetroWarmShadows);
                }

                uint2 pixel = (uint2)(uv * _RetroTargetSize.xy);
                float steps = _RetroColorSteps;
                if (steps < 255)
                {
                    float threshold = (Bayer4(pixel) + 0.5) / 16 - 0.5;
                    col = saturate(col) + threshold * _RetroDither / steps;
                    col = round(saturate(col) * steps) / steps;
                }

                if (_RetroScanlines > 0 && (pixel.y & 1u) == 1u)
                    col *= 1 - 0.5 * _RetroScanlines;

                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
