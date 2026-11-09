using UnityEngine;

namespace GoldCore
{
    /// <summary>Gravity, drag and impact damage for a projectile fired by <see cref="GunSystem"/>.</summary>
    public class ProjectileBallistics : MonoBehaviour
    {
        private Rigidbody rb;
        private float gravity;
        private float drag;
        private float maxLifetime;
        private float currentLifetime;
        private int damage;
        private GameObject attacker;
        private GameObject impactDecal;

        public void SetupBallistics(Rigidbody projectileRb, float gravityValue, float dragValue, float lifetime)
        {
            rb = projectileRb;
            gravity = gravityValue;
            drag = dragValue;
            maxLifetime = lifetime;
        }

        public void SetDamage(int amount, GameObject owner, GameObject decal)
        {
            damage = amount;
            attacker = owner;
            impactDecal = decal;
        }

        private void FixedUpdate()
        {
            if (rb == null) return;
            rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
            rb.linearVelocity -= rb.linearVelocity * drag * Time.fixedDeltaTime;

            currentLifetime += Time.fixedDeltaTime;
            if (currentLifetime >= maxLifetime) Destroy(gameObject);
        }

        private void OnCollisionEnter(Collision collision)
        {
            ContactPoint contact = collision.GetContact(0);
            var info = new DamageInfo(damage, DamageType.Bullet, attacker) { inflictor = gameObject, point = contact.point };
            if (!Damage.Apply(collision.collider, info) &&
                Physics.Raycast(contact.point + contact.normal * 0.1f, -contact.normal, out RaycastHit hit, 0.2f, ~0, QueryTriggerInteraction.Ignore))
                Impacts.Spawn(hit, impactDecal, 3f);
            Destroy(gameObject);
        }
    }
}
