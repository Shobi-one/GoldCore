using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Trauma-based camera shake. Effects add trauma to every shake in range; the shake fades on its own.
    /// It is added to the main camera the first time something shakes, or add it yourself to tune it.
    /// It only moves the camera's local position, and undoes last frame's offset first, so it never
    /// drifts, never fights the mouse look, and copes with a controller that sets the position itself.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    [AddComponentMenu("GoldCore/Camera Shake")]
    public class CameraShake : MonoBehaviour
    {
        [Tooltip("Offset in meters at full trauma.")]
        [Min(0f)] public float maxOffset = 0.25f;
        [Tooltip("Trauma lost per second.")]
        [Min(0.01f)] public float recovery = 1.5f;
        [Min(0.1f)] public float frequency = 25f;
        [Tooltip("Scales every shake, for a settings menu.")]
        [Range(0f, 2f)] public float strength = 1f;

        static readonly System.Collections.Generic.List<CameraShake> active = new System.Collections.Generic.List<CameraShake>();

        float trauma;
        Vector3 applied;
        Vector3 basePosition;
        float seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => active.Clear();

        /// <summary>Adds trauma (0 to 1) to this camera.</summary>
        public void Add(float amount) => trauma = Mathf.Clamp01(trauma + amount);

        /// <summary>Shakes every camera within <paramref name="radius"/>, less the further away it is.</summary>
        public static void AddAt(Vector3 position, float amount, float radius)
        {
            if (amount <= 0f || !Application.isPlaying) return;
            if (active.Count == 0 && Camera.main != null)
                Camera.main.gameObject.AddComponent<CameraShake>();

            foreach (CameraShake shake in active)
            {
                float d = Vector3.Distance(shake.transform.position, position);
                float falloff = radius <= 0f ? 1f : 1f - Mathf.Clamp01(d / radius);
                if (falloff > 0f) shake.Add(amount * falloff);
            }
        }

        /// <summary>Shakes every camera by the same amount, for something like a weapon kick.</summary>
        public static void AddGlobal(float amount)
        {
            if (amount <= 0f || !Application.isPlaying) return;
            if (active.Count == 0 && Camera.main != null)
                Camera.main.gameObject.AddComponent<CameraShake>();
            foreach (CameraShake shake in active) shake.Add(amount);
        }

        void Awake() => seed = Random.value * 100f;

        void OnEnable()
        {
            active.Add(this);
            basePosition = transform.localPosition;
            applied = Vector3.zero;
        }

        void OnDisable()
        {
            active.Remove(this);
            transform.localPosition -= applied;
            applied = Vector3.zero;
            trauma = 0f;
        }

        void LateUpdate()
        {
            // If something else moved the camera since last frame, its new position is the base.
            Vector3 current = transform.localPosition;
            basePosition = current == basePosition + applied ? basePosition : current;
            applied = Vector3.zero;
            transform.localPosition = basePosition;
            if (trauma <= 0f) return;

            float power = trauma * trauma * maxOffset * strength;
            float t = Time.time * frequency;
            applied = new Vector3(
                (Mathf.PerlinNoise(seed, t) * 2f - 1f) * power,
                (Mathf.PerlinNoise(seed + 1f, t) * 2f - 1f) * power,
                0f);
            transform.localPosition = basePosition + applied;
            trauma = Mathf.Max(0f, trauma - recovery * Time.deltaTime);
        }
    }
}
