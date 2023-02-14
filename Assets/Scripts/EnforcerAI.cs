using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnforcerAI : MonoBehaviour
{
    // Variables
    public bool canShoot = true;
    public float shootingRange = 10f;
    public float fireRate = 1f;
    public int amount = 10;
    public int maxHP = 40;
    private int currentHP;
    private float nextFire;
    //private Animator anim;
    public GameObject bloodDecalPrefab;
    private NavMeshAgent agent;
    public GameObject player;
    public Transform playerT;

    void Start()
    {
        currentHP = maxHP;
        //anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent.velocity.magnitude > 0)
        {
            canShoot = false;
            //anim.SetBool("isRunning", true);
        }
        else
        {
            canShoot = true;
            //anim.SetBool("isRunning", false);
        }

        if (canShoot)
        {
            float distance = Vector3.Distance(transform.position, playerT.position);
            if (distance <= shootingRange)
            {
                LookAtPlayer();
                if (Time.time > nextFire)
                {
                    nextFire = Time.time + fireRate;
                    if (Shoot())
                    {
                        //anim.SetTrigger("shoot");
                    }
                }
            }
        }
    }

    void LookAtPlayer()
    {
        Vector3 direction = (playerT.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    bool Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out hit, shootingRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                agent.isStopped = true;
                player.GetComponent<HealthManager>().DecreaseHealth(amount);
            }
        }
        return false;
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

    void OnDrawGizmosSelected()
    {
        // Draw a red sphere to represent the attacking range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);
    }
}
