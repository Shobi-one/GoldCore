using System.Collections.Generic;
using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// A particle effect described as data: layers of particles, plus an optional light flash,
    /// camera shake and sound. Make one with Assets > Create > GoldCore > Effect, or duplicate a
    /// shipped preset and change its values. Play it with <see cref="Effects.Play"/> or an <see cref="EffectPlayer"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Effect", menuName = "GoldCore/Effect", order = 20)]
    public class EffectPreset : ScriptableObject
    {
        [Tooltip("Keep emitting until stopped, for fire and smoke. One-shot effects clean themselves up.")]
        public bool loop;
        [Tooltip("Scales sizes, speeds and radii of every layer. A small explosion is the big one at 0.5.")]
        [Min(0.01f)] public float scale = 1f;
        [Tooltip("How many copies can play at once. The oldest is reused past this.")]
        [Min(1)] public int maxInstances = 16;

        public List<EffectLayer> layers = new List<EffectLayer>();

        [Header("Light flash")]
        public bool flash;
        public Color flashColor = new Color(1f, 0.7f, 0.4f);
        [Min(0f)] public float flashIntensity = 4f;
        [Min(0f)] public float flashRange = 6f;
        [Min(0.01f)] public float flashDuration = 0.15f;

        [Header("Camera shake")]
        [Tooltip("Shake strength for a camera standing at the effect. 0 is off.")]
        [Range(0f, 1f)] public float shake = 0f;
        [Tooltip("Distance where the shake fades to nothing.")]
        [Min(0f)] public float shakeRadius = 15f;

        [Header("Sound")]
        public AudioClip[] sounds;
        [Range(0f, 1f)] public float volume = 1f;
        public FloatRange pitch = new FloatRange(0.95f, 1.05f);
        [Min(1f)] public float soundRange = 40f;

        /// <summary>Changes every time the preset is edited, so playing effects can rebuild.</summary>
        public int Version { get; private set; }

        /// <summary>How long a one-shot effect lasts, in seconds.</summary>
        public float Duration
        {
            get
            {
                float d = flash ? flashDuration : 0f;
                foreach (EffectLayer layer in layers)
                    if (layer != null && layer.enabled) d = Mathf.Max(d, layer.MaxDuration);
                return d;
            }
        }

        public AudioClip RandomSound() =>
            sounds == null || sounds.Length == 0 ? null : sounds[Random.Range(0, sounds.Length)];

        void OnValidate() => Version++;
    }
}
