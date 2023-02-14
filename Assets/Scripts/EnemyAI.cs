//THIS CODE IS OUTDATED... THIS WILL BE RECODED IN THE FUTURE
//PLEASE OBI STOP WORKING ON THIS AND MAKE ANOTHER SCRIPT!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    private Animator animator;
    private int currentHP;
    private bool isAttacking = false;
    private float lastAttackTime;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHP = maxHP;
    }

    void Attack()
    {
        // Check if the attack interval has passed
        if (Time.time - lastAttackTime >= attackInterval && !isAttacking)
        {
            isAttacking = true;
            navMeshAgent.isStopped = true;
            lastAttackTime = Time.time;
            StartCoroutine(PerformAttack());
        }
    }

    bool IsPlayerInView()
    {
        // TODO: Implement code to check if the player is within the zombie's field of view
        return true;
    }

    bool IsPlayerInAttackRange()
    {
        return Vector3.Distance(transform.position, playerTransform.position) <= attackRange;
    }

    public void TakeDamage(int damage)
    {
        // Reduce the zombie's current HP by the amount of damage taken
        currentHP -= damage;
        Debug.Log(currentHP);

        // If the zombie's HP is 0 or less, destroy it
        if (currentHP <= 0)
        {
            Destroy(gameObject);
        }
    }
    IEnumerator PerformAttack()
    {
        // Wait for the attack interval to elapse
        yield return new WaitForSeconds(attackInterval);
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            player.GetComponent<HealthManager>().DecreaseHealth(amount);
        }
        isAttacking = false;
        navMeshAgent.isStopped = false;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a red sphere to represent the attacking range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
