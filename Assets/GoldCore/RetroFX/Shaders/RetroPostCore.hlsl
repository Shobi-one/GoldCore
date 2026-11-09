// The RetroFX image effect, shared by the built-in and URP shaders.
// Before including this, define:
//   RETRO_SAMPLE(uv)     returns the source color (float3) at uv
//   RETRO_EYE_DEPTH(uv)  returns the eye space depth at uv
#ifndef GOLDCORE_RETRO_POST_CORE
#define GOLDCORE_RETRO_POST_CORE

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
static const float RetroBayer[16] = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

float3 RetroBright(float2 uv)
{
    float3 c = RETRO_SAMPLE(uv);
    float l = max(c.r, max(c.g, c.b));
    return c * saturate((l - _RetroBloomThreshold) / max(1e-4, 1 - _RetroBloomThreshold));
}

float4 RetroProcess(float2 uv)
{
    float2 px = _RetroTargetSize.zw;   // one output pixel in uv
    float3 col = RETRO_SAMPLE(uv);

    if (_RetroBlur > 0)
    {
        float2 o = px * 0.75;
        float3 b = RETRO_SAMPLE(uv + float2(o.x, o.y)) + RETRO_SAMPLE(uv + float2(-o.x, o.y))
                 + RETRO_SAMPLE(uv + float2(o.x, -o.y)) + RETRO_SAMPLE(uv + float2(-o.x, -o.y));
        col = lerp(col, b * 0.25, _RetroBlur);
    }

    if (_RetroFogRange.x > 0)
    {
        float dist = RETRO_EYE_DEPTH(uv);
        float f = saturate((dist - _RetroFogRange.y) / (_RetroFogRange.z - _RetroFogRange.y));
        col = lerp(col, _RetroFogColor.rgb, f * _RetroFogColor.a);
    }

    if (_RetroBloom > 0)
    {
        float2 r = px * 3;
        float3 g = RetroBright(uv + float2(r.x, 0)) + RetroBright(uv - float2(r.x, 0))
                 + RetroBright(uv + float2(0, r.y)) + RetroBright(uv - float2(0, r.y))
                 + RetroBright(uv + r * 0.7) + RetroBright(uv - r * 0.7)
                 + RetroBright(uv + float2(r.x, -r.y) * 0.7) + RetroBright(uv + float2(-r.x, r.y) * 0.7);
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
        float threshold = (RetroBayer[(pixel.y & 3u) * 4u + (pixel.x & 3u)] + 0.5) / 16 - 0.5;
        col = saturate(col) + threshold * _RetroDither / steps;
        col = round(saturate(col) * steps) / steps;
    }

    if (_RetroScanlines > 0 && (pixel.y & 1u) == 1u)
        col *= 1 - 0.5 * _RetroScanlines;

    return float4(col, 1);
}

#endif
