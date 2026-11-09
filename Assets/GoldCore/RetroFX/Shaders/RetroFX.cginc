// Shared helpers for GoldCore/RetroLit. RetroCamera sets the globals from its preset.
#ifndef GOLDCORE_RETROFX_INCLUDED
#define GOLDCORE_RETROFX_INCLUDED

float _RetroVertexSnap;   // grid cells over the screen height, 0 = off
float _RetroAffine;       // 0 = perspective correct, 1 = fully affine
float _RetroPointSample;  // 1 = hard texels

// Snaps a clip space position to a screen grid, so vertices jump between positions.
float4 RetroSnapVertex(float4 clipPos, float overrideCells)
{
    float cells = overrideCells > 0 ? overrideCells : _RetroVertexSnap;
    if (cells <= 0 || clipPos.w <= 0) return clipPos;
    float2 grid = float2(cells * (_ScreenParams.x / _ScreenParams.y), cells) * 0.5;
    float2 ndc = clipPos.xy / clipPos.w;
    ndc = floor(ndc * grid + 0.5) / grid;
    clipPos.xy = ndc * clipPos.w;
    return clipPos;
}

// Pass uv * w and w to the fragment stage; dividing them there gives affine mapping.
float3 RetroAffineUV(float2 uv, float4 clipPos)
{
    return float3(uv * clipPos.w, clipPos.w);
}

float2 RetroResolveUV(float2 perspectiveUV, float3 affine, float4 texelSize)
{
    float2 uv = lerp(perspectiveUV, affine.xy / affine.z, _RetroAffine);
    if (_RetroPointSample > 0)
        uv = (floor(uv * texelSize.zw) + 0.5) * texelSize.xy;
    return uv;
}

#endif
