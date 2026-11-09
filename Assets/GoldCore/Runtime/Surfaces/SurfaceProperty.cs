using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// How a material reacts to hits and footsteps, like an entry in Source's surfaceproperties.txt.
    /// Make one per material (concrete, metal, wood, flesh) with Assets > Create > GoldCore > Surface Property,
    /// then put a <see cref="SurfaceTag"/> on the colliders that use it.
    /// </summary>
    [CreateAssetMenu(fileName = "Surface", menuName = "GoldCore/Surface Property", order = 10)]
    public class SurfaceProperty : ScriptableObject
    {
        [Header("Impacts")]
        [Tooltip("Decal placed where a bullet or melee hit lands, such as a bullet hole.")]
        public GameObject impactDecal;
        [Min(0f)] public float decalLifetime = 10f;
        [Tooltip("Particle effect spawned at the hit, facing out of the surface.")]
        public GameObject impactEffect;
        public AudioClip[] impactSounds;

        [Header("Footsteps")]
        public AudioClip[] footstepSounds;
        [Range(0f, 1f)] public float footstepVolume = 0.5f;

        public AudioClip RandomImpactSound() => Pick(impactSounds);
        public AudioClip RandomFootstep() => Pick(footstepSounds);

        static AudioClip Pick(AudioClip[] clips) =>
            clips == null || clips.Length == 0 ? null : clips[Random.Range(0, clips.Length)];
    }
}
