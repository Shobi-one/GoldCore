using UnityEngine;

namespace GoldCore
{
    /// <summary>Helpers that let the old NPC scripts use <see cref="Health"/>.</summary>
    public static class NpcHealth
    {
        /// <summary>Returns the GameObject's Health, adding one with this max if it has none.</summary>
        public static Health Ensure(GameObject go, int maxHealth)
        {
            if (go.TryGetComponent(out Health health)) return health;
            health = go.AddComponent<Health>();
            health.maxHealth = maxHealth;
            health.currentHealth = maxHealth;
            health.maxArmor = 0;
            health.destroyOnDeath = true;
            return health;
        }

        /// <summary>Fills in the player references from the "Player" tag when they were left empty.</summary>
        public static void FindPlayer(ref GameObject player, ref Transform playerTransform)
        {
            if (player == null)
            {
                player = playerTransform != null ? playerTransform.gameObject : GameObject.FindWithTag("Player");
            }
            if (playerTransform == null && player != null) playerTransform = player.transform;
        }
    }
}
