using System.Collections.Generic;
using UnityEngine;

namespace GoldCore
{
    /// <summary>Turns an <see cref="EffectLayer"/> into ParticleSystem settings.</summary>
    public static class EffectBuilder
    {
        public const string ShaderName = "Hidden/GoldCore/Particle";
        const string FallbackShader = "Universal Render Pipeline/Particles/Unlit";

        static readonly int ShapeId = Shader.PropertyToID("_Shape");
        static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        static readonly int PixelateId = Shader.PropertyToID("_Pixelate");
        static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
        static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
        static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        static readonly int AdditiveId = Shader.PropertyToID("_Additive");

        static Shader shader;
        static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            shader = null;
            materials.Clear();
        }

        /// <summary>Builds the particle systems for a preset under <paramref name="root"/>.</summary>
        public static ParticleSystem[] Build(EffectPreset preset, Transform root, float scale)
        {
            var systems = new List<ParticleSystem>();
            foreach (EffectLayer layer in preset.layers)
            {
                if (layer == null || !layer.enabled) continue;
                var go = new GameObject(string.IsNullOrEmpty(layer.name) ? "Layer" : layer.name);
                go.transform.SetParent(root, false);
                go.layer = root.gameObject.layer;
                var ps = go.AddComponent<ParticleSystem>();
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Apply(ps, layer, preset.loop, preset.scale * scale);
                systems.Add(ps);
            }
            return systems.ToArray();
        }

        public static void Apply(ParticleSystem ps, EffectLayer l, bool loop, float scale)
        {
            float maxLife = Mathf.Max(0.01f, Mathf.Max(l.lifetime.min, l.lifetime.max));

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = loop;
            main.duration = 1f;
            main.startDelay = l.delay;
            main.startLifetime = new ParticleSystem.MinMaxCurve(l.lifetime.min, l.lifetime.max);
            main.startSpeed = new ParticleSystem.MinMaxCurve(l.speed.min * scale, l.speed.max * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(l.size.min * scale, l.size.max * scale);
            main.startColor = Color.white;
            main.startRotation = l.randomRotation
                ? new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f)
                : new ParticleSystem.MinMaxCurve(0f);
            main.gravityModifier = l.gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            // Local: a scaled weapon model or prop never resizes the effect. Use the preset's Scale instead.
            main.scalingMode = ParticleSystemScalingMode.Local;
            main.maxParticles = Mathf.Max(1, l.burst + Mathf.CeilToInt(l.rate * maxLife * 1.5f));
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = l.rate;
            emission.SetBursts(l.burst > 0
                ? new[] { new ParticleSystem.Burst(0f, (short)Mathf.Min(l.burst, short.MaxValue)) }
                : new ParticleSystem.Burst[0]);

            var shape = ps.shape;
            shape.enabled = true;
            shape.radius = Mathf.Max(0.0001f, l.radius * scale);
            switch (l.emitter)
            {
                case EmitterShape.Point:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.0001f;
                    break;
                case EmitterShape.Sphere:
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    break;
                case EmitterShape.Hemisphere:
                    shape.shapeType = ParticleSystemShapeType.Hemisphere;
                    break;
                case EmitterShape.Cone:
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = l.coneAngle;
                    break;
            }

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(l.startColor, 0f), new GradientColorKey(l.endColor, 1f) },
                new[] { new GradientAlphaKey(l.startColor.a, 0f), new GradientAlphaKey(l.endColor.a, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = !Mathf.Approximately(l.endSize, 1f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f,
                new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, l.endSize)));

            var rotation = ps.rotationOverLifetime;
            rotation.enabled = l.spin.min != 0f || l.spin.max != 0f;
            rotation.z = new ParticleSystem.MinMaxCurve(l.spin.min * Mathf.Deg2Rad, l.spin.max * Mathf.Deg2Rad);

            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = l.drag > 0f;
            limit.limit = new ParticleSystem.MinMaxCurve(10000f);
            limit.dampen = 0f;
            limit.drag = new ParticleSystem.MinMaxCurve(l.drag);

            var collision = ps.collision;
            collision.enabled = l.bounce;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.bounce = new ParticleSystem.MinMaxCurve(0.4f);
            collision.dampen = new ParticleSystem.MinMaxCurve(0.3f);
            collision.quality = ParticleSystemCollisionQuality.Medium;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = l.stretch ? ParticleSystemRenderMode.Stretch : ParticleSystemRenderMode.Billboard;
            renderer.velocityScale = l.stretch ? l.stretchAmount : 0f;
            renderer.lengthScale = 1f;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.sharedMaterial = MaterialFor(l);
        }

        /// <summary>Materials are shared by every layer with the same look.</summary>
        public static Material MaterialFor(EffectLayer l)
        {
            Texture2D tex = l.shape == ParticleShape.Texture ? l.texture : null;
            string key = $"{(int)l.shape}|{(int)l.blend}|{l.intensity:0.###}|{l.pixelate}|{(tex != null ? tex.GetInstanceID() : 0)}";
            if (materials.TryGetValue(key, out Material cached) && cached != null) return cached;

            if (shader == null) shader = Shader.Find(ShaderName);
            if (shader == null) shader = Shader.Find(FallbackShader);
            var mat = new Material(shader) { name = "GoldCore Particle " + key, hideFlags = HideFlags.DontSave };
            mat.SetFloat(ShapeId, (float)l.shape);
            mat.SetFloat(IntensityId, l.intensity);
            mat.SetFloat(PixelateId, l.pixelate);
            bool additive = l.blend == EffectBlend.Additive;
            mat.SetFloat(SrcBlendId, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat(DstBlendId, (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            mat.SetFloat(AdditiveId, additive ? 1f : 0f);
            if (tex != null) mat.SetTexture(MainTexId, tex);
            mat.renderQueue = 3000;
            materials[key] = mat;
            return mat;
        }
    }
}
