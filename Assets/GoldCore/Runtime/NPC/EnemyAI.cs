// Old melee enemy, kept working until the NPC phase replaces it with NpcBase.
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GoldCore
{
    public class EnemyAI : MonoBehaviour
    {
        public Transform playerTransform;
        public float attackRange = 2.5f;
        public float attackInterval = 1.5f;
        public int amount = 10;
        public int maxHP = 50;
        public GameObject player;
        public GameObject bloodDecalPrefab;

        private NavMeshAgent navMeshAgent;
        private Health health;
        private bool isAttacking;
        private float lastAttackTime;

        void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            health = NpcHealth.Ensure(gameObject, maxHP);
            NpcHealth.FindPlayer(ref player, ref playerTransform);
        }

        void Update()
        {
            if (playerTransform == null || health.IsDead) return;
            if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange) Attack();
        }

        void Attack()
        {
            if (Time.time - lastAttackTime < attackInterval || isAttacking) return;
            isAttacking = true;
            if (navMeshAgent != null) navMeshAgent.isStopped = true;
            lastAttackTime = Time.time;
            StartCoroutine(PerformAttack());
        }

        /// <summary>Kept for older callers. New code should use <see cref="Damage.Apply"/>.</summary>
        public void TakeDamage(int damage) => health.TakeDamage(new DamageInfo(damage));

        IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(attackInterval);
            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                var target = player.GetComponentInParent<IDamageable>();
                target?.TakeDamage(new DamageInfo(amount, DamageType.Slash, gameObject) { point = playerTransform.position });
            }
            isAttacking = false;
            if (navMeshAgent != null) navMeshAgent.isStopped = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
