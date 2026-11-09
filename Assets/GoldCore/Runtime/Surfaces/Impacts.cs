using UnityEngine;

namespace GoldCore
{
    /// <summary>Spawns decals, effects and sounds where a bullet or melee hit lands.</summary>
    public static class Impacts
    {
        const float DecalOffset = 0.01f;

        /// <summary>
        /// Uses the hit collider's <see cref="SurfaceTag"/>. Without one, it falls back to
        /// <paramref name="fallbackDecal"/>, so untagged geometry still gets bullet holes.
        /// </summary>
        public static void Spawn(RaycastHit hit, GameObject fallbackDecal = null, float fallbackLifetime = 10f)
        {
            SurfaceProperty surface = SurfaceTag.Find(hit.collider);
            GameObject decal = surface != null && surface.impactDecal != null ? surface.impactDecal : fallbackDecal;
            float lifetime = surface != null && surface.impactDecal != null ? surface.decalLifetime : fallbackLifetime;

            if (decal != null)
            {
                GameObject d = Object.Instantiate(decal, hit.point + hit.normal * DecalOffset, Quaternion.LookRotation(hit.normal));
                d.transform.SetParent(hit.collider.transform, true);
                if (lifetime > 0f) Object.Destroy(d, lifetime);
            }

            if (surface == null) return;
            if (surface.impactEffect != null)
                Object.Destroy(Object.Instantiate(surface.impactEffect, hit.point, Quaternion.LookRotation(hit.normal)), 5f);
            AudioClip clip = surface.RandomImpactSound();
            if (clip != null) AudioSource.PlayClipAtPoint(clip, hit.point);
        }
    }
}
