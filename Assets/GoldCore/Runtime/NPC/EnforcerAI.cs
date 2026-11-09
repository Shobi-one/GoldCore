// Old ranged enemy, kept working until the NPC phase replaces it with NpcBase.
using UnityEngine;
using UnityEngine.AI;

namespace GoldCore
{
    public class EnforcerAI : MonoBehaviour
    {
        public bool canShoot = true;
        public float shootingRange = 10f;
        public float fireRate = 1f;
        public int amount = 10;
        public int maxHP = 40;
        public GameObject bloodDecalPrefab;
        public GameObject player;
        public Transform playerT;

        private float nextFire;
        private NavMeshAgent agent;
        private Health health;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            health = NpcHealth.Ensure(gameObject, maxHP);
            NpcHealth.FindPlayer(ref player, ref playerT);
        }

        void Update()
        {
            if (playerT == null || health.IsDead) return;
            canShoot = agent == null || agent.velocity.sqrMagnitude <= 0.0001f;
            if (!canShoot) return;

            if (Vector3.Distance(transform.position, playerT.position) > shootingRange) return;
            LookAtPlayer();
            if (Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                Shoot();
            }
        }

        void LookAtPlayer()
        {
            Vector3 direction = playerT.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }

        void Shoot()
        {
            if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, shootingRange, ~0, QueryTriggerInteraction.Ignore)
                && hit.transform.CompareTag("Player"))
            {
                if (agent != null) agent.isStopped = true;
                Damage.Apply(hit.collider, new DamageInfo(amount, DamageType.Bullet, gameObject) { point = hit.point });
            }
        }

        /// <summary>Kept for older callers. New code should use <see cref="Damage.Apply"/>.</summary>
        public void TakeDamage(int damage) => health.TakeDamage(new DamageInfo(damage));

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, shootingRange);
        }
    }
}
