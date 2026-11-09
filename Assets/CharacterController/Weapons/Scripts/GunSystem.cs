using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class GunSystem : MonoBehaviour
{
    public enum GunType { Pistol, Shotgun, Rifle }
    public enum FireMode { Hitscan, Projectile }

    [Header("Gun Settings")]
    public GunType gunType;
    public FireMode fireMode = FireMode.Hitscan;  // Default to hitscan
    public bool autoReload = true;
    public int damage;
    public float timeBetweenShooting, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap = 1;
    public int magazineBackup;
    public bool allowButtonHold;
    public bool isBurstFire;
    public int burstCount = 3;
    public float bulletSpread = 0.05f;

    [Header("Shotgun Settings")]
    public float shotgunSpread;
    public int shotgunPellets = 5;

    [Header("Ballistics Settings")]
    public GameObject projectilePrefab;
    public float projectileInitialSpeed = 50f;
    public float projectileGravity = 9.81f;
    public float projectileDrag = 0.1f;
    public float maxLifetime = 5f;

    [Header("UI & Effects")]
    public TextMeshProUGUI currentMagText, backupMagText;
    public Image hudImage;
    public GameObject muzzleFlash, bulletHoleGraphic, bloodDecalPrefab;
    public float camShakeMagnitude, camShakeDuration;


    [Header("References")]
    public GameObject gun;
    public Transform attackPoint;
    public AudioSource gunSound;

    [SerializeField] private Transform fpsCam;

    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;
    private bool ableToShoot => bulletsLeft > 0 && readyToShoot && !reloading;

    [Header("Weapon Sway Settings")]
    public float swayAmount = 0.02f;
    public float swaySmoothness = 5f;
    public float swayRotationAmount = 4f;

    private Vector3 initialGunPosition;
    private Vector3 initialGunRotation;

    private void Awake()
    {
        if (fpsCam == null) fpsCam = Camera.main.transform; // Ensure the camera is assigned
        SetupGunInInventory(false);
        bulletsLeft = magazineSize;
        readyToShoot = true;
        initialGunPosition = gun.transform.localPosition;
        initialGunRotation = gun.transform.localEulerAngles;
    }

    private void Update()
    {
        HandleInput();
        UpdateUI();
        ApplyWeaponSway();

        if (autoReload && bulletsLeft == 0 && !reloading && magazineBackup > 0)
        {
            StartCoroutine(Reload());
        }
    }
    private void HandleInput()
    {
        shooting = allowButtonHold ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading && magazineBackup > 0)
        {
            StartCoroutine(Reload());
        }

        if (ableToShoot && shooting)
        {
            if (isBurstFire)
            {
                StartCoroutine(BurstFire());
            }
            else
            {
                bulletsShot = bulletsPerTap;
                Shoot();
            }
        }
    }

    private IEnumerator BurstFire()
    {
        for (int i = 0; i < burstCount; i++)
        {
            if (ableToShoot)
            {
                Shoot();
                yield return new WaitForSeconds(timeBetweenShots); // Wait between each shot in the burst
            }
            else
            {
                break; // Stop if we can't shoot anymore
            }
        }
    }

    private void Shoot()
    {
        readyToShoot = false;
        gunSound.Play();

        if (gunType == GunType.Shotgun)
        {
            ShootShotgun();
        }
        else
        {
            if (fireMode == FireMode.Hitscan)
            {
                ShootHitscan();
            }
            else if (fireMode == FireMode.Projectile)
            {
                ShootProjectileWithBallistics();
            }
        }

        bulletsLeft--;
        StartCoroutine(ResetShoot());
    }

    private void ShootHitscan()
    {
        Vector3 directionWithSpread = ApplySpread(fpsCam.forward, bulletSpread);
        Ray ray = new Ray(fpsCam.position, directionWithSpread);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            ApplyHitEffects(hit);
        }

        GameObject muzzleFlashInstance = Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
        Destroy(muzzleFlashInstance, 0.05f);
    }

    private void ShootProjectileWithBallistics()
    {
        GameObject projectileInstance = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
        Rigidbody rb = projectileInstance.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 directionWithSpread = ApplySpread(fpsCam.forward, bulletSpread);
            rb.linearVelocity = directionWithSpread * projectileInitialSpeed;
            rb.useGravity = false;

            ProjectileBallistics ballistics = projectileInstance.AddComponent<ProjectileBallistics>();
            ballistics.SetupBallistics(rb, projectileGravity, projectileDrag, maxLifetime);
        }

        GameObject muzzleFlashInstance = Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
        Destroy(muzzleFlashInstance, 0.05f);
    }

    private Vector3 ApplySpread(Vector3 direction, float spread)
    {
        // Calculate a spread angle by applying a random offset
        direction += Random.insideUnitSphere * spread;
        return direction.normalized;
    }

    private void ShootShotgun()
    {
        for (int i = 0; i < shotgunPellets; i++)
        {
            Vector3 spread = fpsCam.forward + Random.insideUnitSphere * shotgunSpread;
            Ray ray = new Ray(fpsCam.position, spread);
            if (Physics.Raycast(ray, out RaycastHit hit, range))
            {
                ApplyHitEffects(hit);
            }
        }
        GameObject muzzleFlashInstance = Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
        Destroy(muzzleFlashInstance, 0.05f);
    }

    private void ApplyHitEffects(RaycastHit hit)
    {
        var enemy = hit.transform.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            CreateDecal(bloodDecalPrefab, hit);
        }
        else if (hit.collider.CompareTag("Wall"))
        {
            CreateDecal(bulletHoleGraphic, hit);
        }
    }

    private void CreateDecal(GameObject decalPrefab, RaycastHit hit)
    {
        Vector3 offset = hit.normal * 0.01f;
        GameObject decal = Instantiate(decalPrefab, hit.point + offset, Quaternion.LookRotation(hit.normal));
        decal.transform.SetParent(hit.transform);

        Destroy(decal, 3f);
    }

    private IEnumerator ResetShoot()
    {
        yield return new WaitForSeconds(timeBetweenShots);
        readyToShoot = true;
    }

    private IEnumerator Reload()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);

        int bulletsToLoad = Mathf.Min(magazineSize - bulletsLeft, magazineBackup);
        bulletsLeft += bulletsToLoad;
        magazineBackup -= bulletsToLoad;

        reloading = false;
    }

    private void UpdateUI()
    {
        currentMagText.SetText(bulletsLeft.ToString());
        backupMagText.SetText(magazineBackup.ToString());

        // Color indicators based on ammo
        Color magColor = bulletsLeft > 0 ? new Color32(251, 218, 85, 255) : Color.red;
        currentMagText.color = magColor;
        backupMagText.color = magColor;
        hudImage.color = bulletsLeft > 0 ? Color.white : Color.red;

        // Show/hide settings based on gun type
        ShowGunTypeSettings();
    }

    private void ShowGunTypeSettings()
    {
        // You can implement UI logic here based on the selected gun type
        // For example:
        if (gunType == GunType.Shotgun)
        {
            // Show shotgun-specific settings in the UI
        }
        else if (gunType == GunType.Rifle)
        {
            // Show rifle-specific settings in the UI
        }
        else if (gunType == GunType.Pistol)
        {
            // Show pistol-specific settings in the UI
        }
    }

    private void SetupGunInInventory(bool inInventory)
    {
        gun.SetActive(inInventory);
        hudImage.enabled = inInventory;
        currentMagText.enabled = inInventory;
        backupMagText.enabled = inInventory;
    }

    public void IncreaseAmmo(int amount)
    {
        magazineBackup += amount;
    }

    private void ApplyWeaponSway()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Calculate target position based on mouse movement
        Vector3 targetPosition = initialGunPosition + new Vector3(-mouseX * swayAmount, -mouseY * swayAmount, 0);

        // Smoothly move the gun's position
        gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "GunSystem.icon.png", true);
    }
}
