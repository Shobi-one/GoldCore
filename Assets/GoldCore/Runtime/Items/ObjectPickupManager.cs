using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Health, armor or ammo pickup. Only the player can take it. If the player is full it stays,
    /// like HL2's health kits. Replaced by item_healthkit and friends in the entities phase.
    /// </summary>
    public class ObjectPickupManager : MonoBehaviour
    {
        [Tooltip("Optional. Any object with the player's HealthManager can pick this up.")]
        public GameObject player;
        public bool HealthPickup;
        public bool ArmorPickup;
        public bool AmmoPickup;
        [Space]
        public int amount;
        [Tooltip("Optional. Ammo goes to this gun; empty means the player's current gun.")]
        public GameObject weapons;

        private void OnTriggerEnter(Collider other)
        {
            HealthManager health = other.GetComponentInParent<HealthManager>();
            if (health == null || (player != null && health.gameObject != player)) return;

            bool used = false;
            if (HealthPickup) used |= health.Heal(amount);
            if (ArmorPickup) used |= health.AddArmor(amount);
            if (AmmoPickup)
            {
                GunSystem gun = weapons != null ? weapons.GetComponent<GunSystem>() : health.GetComponentInChildren<GunSystem>();
                if (gun != null)
                {
                    gun.IncreaseAmmo(amount);
                    used = true;
                }
            }

            if (used) Destroy(gameObject);
        }
    }
}
