using System;
using UnityEngine;

namespace GoldCore
{
    /// <summary>A random value between <see cref="min"/> and <see cref="max"/>.</summary>
    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Random() => UnityEngine.Random.Range(min, max);
        public FloatRange Scaled(float s) => new FloatRange(min * s, max * s);
    }

    /// <summary>The procedural sprite each particle draws. No textures needed.</summary>
    public enum ParticleShape
    {
        SoftDot = 0,
        HardDot = 1,
        Ring = 2,
        Smoke = 3,
        Spark = 4,
        Square = 5,
        Texture = 6,
    }

    public enum EffectBlend { Additive, Alpha }

    public enum EmitterShape { Point, Sphere, Hemisphere, Cone }

    /// <summary>
    /// One group of particles in an <see cref="EffectPreset"/>: an explosion is a flash layer,
    /// a fireball layer, a smoke layer and a sparks layer. Every value can be changed in the inspector.
    /// </summary>
    [Serializable]
    public class EffectLayer
    {
        public string name = "Layer";
        public bool enabled = true;

        [Header("Look")]
        public ParticleShape shape = ParticleShape.SoftDot;
        public EffectBlend blend = EffectBlend.Additive;
        [Tooltip("Only used when Shape is Texture.")]
        public Texture2D texture;
        [Tooltip("Color when a particle is born. Alpha is opacity.")]
        public Color startColor = Color.white;
        [Tooltip("Color it fades to by the end of its life.")]
        public Color endColor = new Color(1f, 1f, 1f, 0f);
        [Tooltip("Brightness multiplier. Above 1 glows with bloom.")]
        [Min(0f)] public float intensity = 1f;
        [Tooltip("Draw each particle as a grid of this many pixels across, for a retro look. 0 is smooth.")]
        [Range(0, 32)] public int pixelate = 0;
        [Tooltip("Stretch particles along their velocity, for sparks and tracers.")]
        public bool stretch;
        [Min(0f)] public float stretchAmount = 0.05f;

        [Header("Emission")]
        [Tooltip("Particles spawned at once when the effect plays. On looping presets it repeats every second.")]
        [Min(0)] public int burst = 10;
        [Tooltip("Particles per second while the effect runs. Only useful on looping presets.")]
        [Min(0f)] public float rate = 0f;
        [Tooltip("Seconds after the effect starts before this layer starts.")]
        [Min(0f)] public float delay = 0f;

        [Header("Particles")]
        public FloatRange lifetime = new FloatRange(0.3f, 0.6f);
        public FloatRange speed = new FloatRange(1f, 3f);
        public FloatRange size = new FloatRange(0.1f, 0.2f);
        [Tooltip("Size at the end of a particle's life, as a multiple of its start size.")]
        [Min(0f)] public float endSize = 1f;
        [Tooltip("Spin in degrees per second.")]
        public FloatRange spin = new FloatRange(0f, 0f);
        public bool randomRotation = true;
        [Tooltip("1 falls like normal gravity, negative rises like smoke.")]
        public float gravity = 0f;
        [Tooltip("Air drag. Higher values slow particles down sooner.")]
        [Min(0f)] public float drag = 0f;
        [Tooltip("Bounce off world geometry. Costs more, use it for sparks and debris.")]
        public bool bounce;

        [Header("Emitter")]
        [Tooltip("Hemisphere and Cone point along the effect's forward axis, which is the surface normal for impacts.")]
        public EmitterShape emitter = EmitterShape.Sphere;
        [Min(0f)] public float radius = 0.1f;
        [Range(0f, 90f)] public float coneAngle = 25f;

        public float MaxDuration => delay + Mathf.Max(lifetime.min, lifetime.max);
    }
}
