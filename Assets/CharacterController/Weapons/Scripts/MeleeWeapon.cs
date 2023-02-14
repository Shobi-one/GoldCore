using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public int damage;
    public float attackRange;
    public float attackRate;
    //public Animator animator;
    //public AudioSource audioSource;
    //public AudioClip attackSound;
    public GameObject bulletHolePrefab;
    public GameObject bloodDecalPrefab;
    public Transform fpsCam;
    private float attackTimer;

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (Input.GetButton("Fire1") && attackTimer <= 0)
        {
            attackTimer = attackRate;
            //animator.SetTrigger("Attack");
            //audioSource.PlayOneShot(attackSound);
            Ray shoot = new Ray(fpsCam.position, fpsCam.forward);
            if (Physics.Raycast(shoot, out RaycastHit hit, attackRange))
            {
                EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    GameObject bloodDecal = Instantiate(bloodDecalPrefab, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                    bloodDecal.transform.SetParent(hit.transform);
                    StartCoroutine(DestroyBloodDecal(bloodDecal));
                }
                else if (hit.collider.tag == "Wall")
                {
                    GameObject bulletHole = Instantiate(bulletHolePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    bulletHole.transform.parent = hit.collider.transform;
                }
            }
        }
    }

    IEnumerator DestroyBloodDecal(GameObject bloodDecal)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(bloodDecal);
    }
}
