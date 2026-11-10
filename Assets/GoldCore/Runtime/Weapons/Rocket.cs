using UnityEngine;

namespace GoldCore
{
    /// <summary>Explodes on contact, with damage that falls off over <see cref="blastRadius"/>.</summary>
    public class Rocket : MonoBehaviour
    {
        [Tooltip("GoldCore effect played where the rocket hits. Plays its own flash, shake and sound.")]
        public EffectPreset explosionEffect;
        [Tooltip("Optional prefab spawned as well, for custom explosions.")]
        public GameObject explosionPrefab;
        public float explosionDuration = 3f;

        [Header("Blast")]
        [Tooltip("Damage at the center. 0 keeps the old visual-only explosion.")]
        public float blastDamage = 0f;
        public float blastRadius = 5f;
        public float blastForce = 10f;

        /// <summary>Who fired it, for kill credit. Set by the launcher.</summary>
        [HideInInspector] public GameObject owner;

        void OnCollisionEnter(Collision collision)
        {
            if (explosionEffect != null)
            {
                Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : -transform.forward;
                Effects.Play(explosionEffect, transform.position, Quaternion.LookRotation(normal));
            }
            if (explosionPrefab != null)
                Destroy(Instantiate(explosionPrefab, transform.position, transform.rotation), explosionDuration);
            if (blastDamage > 0f) Explode(transform.position);
            Destroy(gameObject);
        }

        void Explode(Vector3 center)
        {
            var hitAlready = new System.Collections.Generic.HashSet<IDamageable>();
            foreach (Collider col in Physics.OverlapSphere(center, blastRadius, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 closest = col.ClosestPoint(center);
                float falloff = 1f - Mathf.Clamp01(Vector3.Distance(center, closest) / blastRadius);
                Vector3 push = (closest - center).normalized * blastForce * falloff;

                var damageable = col.GetComponentInParent<IDamageable>();
                if (damageable != null && !hitAlready.Add(damageable))
                {
                    if (col.attachedRigidbody != null) col.attachedRigidbody.AddForceAtPosition(push, closest, ForceMode.Impulse);
                    continue;
                }
                Damage.Apply(col, new DamageInfo(blastDamage * falloff, DamageType.Blast, owner)
                {
                    inflictor = gameObject,
                    point = closest,
                    force = push,
                });
            }
        }

        void OnDrawGizmosSelected()
        {
            if (blastDamage <= 0f) return;
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, blastRadius);
        }
    }
}
