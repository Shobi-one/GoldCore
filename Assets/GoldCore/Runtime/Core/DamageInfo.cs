using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Everything about one hit, passed to <see cref="IDamageable.TakeDamage"/>.
    /// Modeled on Source's CTakeDamageInfo.
    /// </summary>
    [System.Serializable]
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;
        /// <summary>Who caused it (the player, an NPC). Can be null.</summary>
        public GameObject attacker;
        /// <summary>What dealt it (a weapon, a projectile, a trigger). Can be null.</summary>
        public GameObject inflictor;
        public Vector3 point;
        /// <summary>Push applied to a rigidbody that is hit. Zero for none.</summary>
        public Vector3 force;

        public DamageInfo(float amount, DamageType type = DamageType.Generic, GameObject attacker = null)
        {
            this.amount = amount;
            this.type = type;
            this.attacker = attacker;
            inflictor = null;
            point = Vector3.zero;
            force = Vector3.zero;
        }

        public bool Has(DamageType flag) => (type & flag) != 0;
    }
}
