using UnityEngine;

namespace GoldCore
{
    /// <summary>Anything that can be hurt. <see cref="Health"/> is the usual one.</summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo info);
    }

    public static class Damage
    {
        /// <summary>
        /// Damages whatever owns the collider (it looks up the parents too) and pushes its rigidbody.
        /// Returns true when something took the damage.
        /// </summary>
        public static bool Apply(Collider target, DamageInfo info)
        {
            if (target == null) return false;
            if (target.attachedRigidbody != null && info.force != Vector3.zero)
                target.attachedRigidbody.AddForceAtPosition(info.force, info.point, ForceMode.Impulse);

            var damageable = target.GetComponentInParent<IDamageable>();
            if (damageable == null) return false;
            damageable.TakeDamage(info);
            return true;
        }
    }
}
