using System.Collections;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    public int damage;
    public float attackRange;
    public float attackRate;
    public GameObject bulletHolePrefab;
    public GameObject bloodDecalPrefab;
    public Transform fpsCam;

    private float attackTimer;
    private const float DECAL_OFFSET = 0.01f; // Offset to avoid Z-fighting
    private Camera mainCamera;

    // Weapon Sway Settings
    public float swayAmount = 0.02f;
    public float swaySmoothness = 5f;

    private Vector3 initialPosition;

    void Start()
    {
        mainCamera = Camera.main; // Cache the main camera reference
        initialPosition = transform.localPosition; // Store the initial position of the melee weapon
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        // Handle weapon sway
        ApplyWeaponSway();

        if (Input.GetButton("Fire1") && attackTimer <= 0)
        {
            attackTimer = attackRate;
            // Uncomment if using an animator
            // animator.SetTrigger("Attack");
            // audioSource.PlayOneShot(attackSound);
            
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Ray shoot = new Ray(fpsCam.position, fpsCam.forward);
        if (Physics.Raycast(shoot, out RaycastHit hit, attackRange))
        {
            EnemyAI enemy = hit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                CreateBloodDecal(hit.point, hit.normal, hit.transform);
            }
            else if (hit.collider.CompareTag("Wall"))
            {
                CreateBulletHole(hit.point, hit.normal, hit.collider.transform);
            }
        }
    }

    private void CreateBloodDecal(Vector3 position, Vector3 normal, Transform parent)
    {
        Vector3 decalPosition = position + normal * DECAL_OFFSET; // Offset to avoid Z-fighting
        GameObject bloodDecal = Instantiate(bloodDecalPrefab, decalPosition, Quaternion.FromToRotation(Vector3.forward, normal));
        bloodDecal.transform.SetParent(parent);
        StartCoroutine(DestroyBloodDecal(bloodDecal));
    }

    private void CreateBulletHole(Vector3 position, Vector3 normal, Transform parent)
    {
        Vector3 holePosition = position + normal * DECAL_OFFSET; // Offset to avoid Z-fighting
        GameObject bulletHole = Instantiate(bulletHolePrefab, holePosition, Quaternion.LookRotation(normal));
        bulletHole.transform.SetParent(parent);
    }

    private IEnumerator DestroyBloodDecal(GameObject bloodDecal)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(bloodDecal);
    }

    private void ApplyWeaponSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Calculate target position based on mouse movement
        Vector3 targetPosition = initialPosition + new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0);

        // Smoothly move the weapon's position
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
    }
}
