using UnityEngine;
using UnityEngine.Events;

namespace GoldCore
{
    /// <summary>
    /// Health and armor for the player, NPCs or breakable props. Armor works like Half-Life 2:
    /// it takes <see cref="armorAbsorb"/> of each hit, spending one armor point per
    /// <see cref="damagePerArmor"/> damage it soaks up.
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        public int maxHealth = 100;
        public int currentHealth = 100;
        public int maxArmor = 100;
        public int currentArmor = 0;

        [Tooltip("Share of each hit armor takes. Half-Life 2 uses 0.8.")]
        [Range(0f, 1f)] public float armorAbsorb = 0.8f;
        [Tooltip("Damage one armor point soaks up. Half-Life 2 uses 2.")]
        [Min(0.01f)] public float damagePerArmor = 2f;

        [Tooltip("Destroy this GameObject when health reaches 0.")]
        public bool destroyOnDeath = false;
        [Tooltip("Takes no damage, like the console's god command.")]
        public bool godMode = false;

        public UnityEvent<DamageInfo> onDamaged = new UnityEvent<DamageInfo>();
        public UnityEvent onDeath = new UnityEvent();
        /// <summary>Health or armor changed, for HUDs.</summary>
        public UnityEvent onChanged = new UnityEvent();

        public bool IsDead => currentHealth <= 0;
        public DamageInfo LastDamage { get; private set; }

        public virtual void TakeDamage(DamageInfo info)
        {
            if (IsDead || godMode || info.amount <= 0f) return;

            float damage = info.amount;
            if (currentArmor > 0 && !info.Has(DamageType.BypassArmor) && !info.Has(DamageType.Fall) && !info.Has(DamageType.Drown))
            {
                float toHealth = damage * (1f - armorAbsorb);
                float armorCost = (damage - toHealth) / damagePerArmor;
                if (armorCost > currentArmor)
                {
                    // Not enough armor: the part it can't cover goes to health.
                    toHealth = damage - currentArmor * damagePerArmor;
                    armorCost = currentArmor;
                }
                currentArmor = Mathf.Max(0, currentArmor - Mathf.CeilToInt(armorCost));
                damage = toHealth;
            }

            currentHealth = Mathf.Max(0, currentHealth - Mathf.CeilToInt(damage));
            LastDamage = info;
            onDamaged.Invoke(info);
            Changed();

            if (currentHealth == 0)
            {
                onDeath.Invoke();
                Died();
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        /// <summary>Simple damage without a source, kept for old scripts.</summary>
        public void TakeDamage(int amount) => TakeDamage(new DamageInfo(amount));

        /// <summary>Adds health up to the maximum. Returns false if already full.</summary>
        public bool Heal(int amount)
        {
            if (IsDead || currentHealth >= maxHealth) return false;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            Changed();
            return true;
        }

        /// <summary>Adds armor up to the maximum. Returns false if already full.</summary>
        public bool AddArmor(int amount)
        {
            if (currentArmor >= maxArmor) return false;
            currentArmor = Mathf.Min(maxArmor, currentArmor + amount);
            Changed();
            return true;
        }

        public void Kill(GameObject attacker = null) =>
            TakeDamage(new DamageInfo(currentHealth + maxArmor * damagePerArmor, DamageType.BypassArmor, attacker));

        protected virtual void Changed() => onChanged.Invoke();
        protected virtual void Died() { }
    }
}
