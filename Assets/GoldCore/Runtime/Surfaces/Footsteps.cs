using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Plays footstep sounds from the <see cref="SurfaceProperty"/> under the player, at a pace that
    /// follows speed, like Source's player footsteps. Drop it on the Player next to an AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Footsteps : MonoBehaviour
    {
        [Tooltip("Surface used when the ground has no SurfaceTag. Leave empty for silence there.")]
        public SurfaceProperty defaultSurface;
        [Tooltip("Distance walked between steps, in meters.")]
        [Min(0.1f)] public float strideLength = 2.2f;
        [Tooltip("Slower than this (m/s) makes no sound, like walking with the walk key.")]
        public float minSpeed = 1.5f;
        public float groundCheckDistance = 1.3f;
        public LayerMask groundMask = ~0;

        AudioSource source;
        Vector3 lastPosition;
        float distance;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            lastPosition = transform.position;
        }

        void Update()
        {
            Vector3 delta = transform.position - lastPosition;
            lastPosition = transform.position;
            delta.y = 0f;
            float speed = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;

            if (speed < minSpeed || !Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
            {
                distance = strideLength * 0.5f;
                return;
            }

            distance += delta.magnitude;
            if (distance < strideLength) return;
            distance = 0f;

            SurfaceProperty surface = SurfaceTag.Find(hit.collider);
            if (surface == null) surface = defaultSurface;
            if (surface == null) return;
            AudioClip clip = surface.RandomFootstep();
            if (clip != null) source.PlayOneShot(clip, surface.footstepVolume);
        }
    }
}
