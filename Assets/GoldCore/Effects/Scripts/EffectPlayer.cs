using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Drag onto any object to play an <see cref="EffectPreset"/> there: a burning barrel, a smoking
    /// vent, a sparking cable. Call <see cref="Play"/> and <see cref="Stop"/> from a UnityEvent or a button.
    /// </summary>
    [AddComponentMenu("GoldCore/Effect Player")]
    public class EffectPlayer : MonoBehaviour
    {
        public EffectPreset preset;
        [Tooltip("Play when the object is enabled, and stop when it is disabled.")]
        public bool playOnEnable = true;
        [Tooltip("Move with this object. Off leaves one-shot effects where they started.")]
        public bool follow = true;
        [Min(0.01f)] public float scale = 1f;

        EffectInstance current;

        void OnEnable()
        {
            if (playOnEnable) Play();
        }

        void OnDisable() => Stop();

        public void Play()
        {
            if (preset == null) return;
            if (current != null && current.IsPlaying && preset.loop) return;
            current = Effects.Play(preset, transform.position, transform.rotation, follow ? transform : null, scale);
        }

        public void Stop()
        {
            if (current != null && current.IsPlaying) current.Stop();
            current = null;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, 0.15f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.4f);
        }
    }
}
