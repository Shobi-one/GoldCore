using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// One playing copy of an <see cref="EffectPreset"/>. Made and reused by <see cref="Effects"/>;
    /// you only need it to stop a looping effect or move one around.
    /// </summary>
    [AddComponentMenu("")]
    public class EffectInstance : MonoBehaviour
    {
        public EffectPreset Preset { get; private set; }
        public bool IsPlaying { get; private set; }

        internal int builtVersion;
        internal float builtScale;

        ParticleSystem[] systems = new ParticleSystem[0];
        Light flashLight;
        AudioSource audioSource;
        float age;
        bool stopping;

        /// <summary>(Re)creates the particle systems from the preset. Called by the pool and the editor preview.</summary>
        public void Build(EffectPreset preset, float scale)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyNow(transform.GetChild(i).gameObject);

            Preset = preset;
            builtVersion = preset.Version;
            builtScale = scale;
            name = preset.name;
            systems = EffectBuilder.Build(preset, transform, scale);

            if (preset.flash)
            {
                if (flashLight == null) flashLight = gameObject.AddComponent<Light>();
                flashLight.type = LightType.Point;
                flashLight.color = preset.flashColor;
                flashLight.range = preset.flashRange * preset.scale * scale;
                flashLight.shadows = LightShadows.None;
                flashLight.enabled = false;
            }
            else if (flashLight != null)
            {
                flashLight.enabled = false;
            }
        }

        /// <summary>Starts from the beginning. Use <see cref="Effects.Play"/> rather than calling this.</summary>
        public void Play()
        {
            age = 0f;
            stopping = false;
            IsPlaying = true;
            gameObject.SetActive(true);

            foreach (ParticleSystem ps in systems)
            {
                ps.Clear(true);
                ps.Play(true);
            }

            if (flashLight != null && Preset.flash)
            {
                flashLight.intensity = Preset.flashIntensity;
                flashLight.enabled = true;
            }

            AudioClip clip = Preset.RandomSound();
            if (clip != null)
            {
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    audioSource.playOnAwake = false;
                    audioSource.spatialBlend = 1f;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                }
                audioSource.maxDistance = Preset.soundRange;
                audioSource.pitch = Preset.pitch.Random();
                audioSource.PlayOneShot(clip, Preset.volume);
            }

            if (Preset.shake > 0f)
                CameraShake.AddAt(transform.position, Preset.shake, Preset.shakeRadius);
        }

        /// <summary>Stops emitting. Particles already alive fade out, then the instance is reused.</summary>
        public void Stop()
        {
            if (!IsPlaying) return;
            stopping = true;
            foreach (ParticleSystem ps in systems)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        /// <summary>Stops and clears at once.</summary>
        public void Kill()
        {
            foreach (ParticleSystem ps in systems)
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Finish();
        }

        /// <summary>
        /// Advances the light flash and the end check. <see cref="Effects"/> calls it every frame,
        /// and the editor preview calls it with its own time step.
        /// </summary>
        internal void Tick(float dt)
        {
            if (!IsPlaying) return;
            age += dt;

            if (flashLight != null && flashLight.enabled)
            {
                float t = Mathf.Clamp01(age / Preset.flashDuration);
                flashLight.intensity = Preset.flashIntensity * (1f - t) * (1f - t);
                if (t >= 1f) flashLight.enabled = false;
            }

            bool oneShotDone = !Preset.loop && age >= Preset.Duration;
            if ((oneShotDone || stopping) && !AnyAlive()) Finish();
        }

        /// <summary>Advances the effect by hand, for the edit-mode preview.</summary>
        public void Simulate(float dt)
        {
            foreach (ParticleSystem ps in systems)
                ps.Simulate(dt, true, false, false);
            Tick(dt);
        }

        bool AnyAlive()
        {
            foreach (ParticleSystem ps in systems)
                if (ps != null && ps.IsAlive(true)) return true;
            return audioSource != null && audioSource.isPlaying;
        }

        void Finish()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            if (flashLight != null) flashLight.enabled = false;
            Effects.Release(this);
        }

        static void DestroyNow(Object o)
        {
            if (Application.isPlaying) Destroy(o);
            else DestroyImmediate(o);
        }
    }
}
