#ifndef GOLDCORE_PARTICLE_CORE
#define GOLDCORE_PARTICLE_CORE

// Shared by the URP and built-in passes of Hidden/GoldCore/Particle.
// Each particle is a quad with UVs 0..1. The sprite is drawn with math, so presets need no textures.
//   0 SoftDot  1 HardDot  2 Ring  3 Smoke  4 Spark  5 Square  6 Texture

float GCHash(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float GCNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = GCHash(i);
    float b = GCHash(i + float2(1, 0));
    float c = GCHash(i + float2(0, 1));
    float d = GCHash(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float2 GCPixelate(float2 uv, float pixels)
{
    return pixels > 0.5 ? (floor(uv * pixels) + 0.5) / pixels : uv;
}

// Returns the sprite's opacity at uv for the given shape.
float GCShapeAlpha(float2 uv, float shape)
{
    float2 p = uv * 2.0 - 1.0;
    float r = length(p);

    if (shape < 0.5) // SoftDot
    {
        float a = saturate(1.0 - r);
        return a * a;
    }
    if (shape < 1.5) // HardDot
        return r < 0.85 ? 1.0 : 0.0;
    if (shape < 2.5) // Ring
        return saturate(1.0 - abs(r - 0.7) * 6.0);
    if (shape < 3.5) // Smoke
    {
        float n = GCNoise(uv * 4.0) * 0.6 + GCNoise(uv * 9.0 + 3.1) * 0.4;
        float edge = saturate(1.0 - r);
        return saturate(edge * edge * (0.4 + n * 1.2));
    }
    if (shape < 4.5) // Spark: a soft diamond that stretches into a streak
    {
        float a = saturate(1.0 - abs(p.x)) * saturate(1.0 - abs(p.y));
        return a * a;
    }
    return 1.0; // Square and Texture
}

#endif
