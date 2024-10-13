using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GunSystem : MonoBehaviour
{
    public enum GunType { Pistol, Shotgun, Rifle }

    [Header("Gun Settings")]
    public GunType gunType;
    public int damage;
    public float timeBetweenShooting, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap = 1;
    public int magazineBackup;
    public bool allowButtonHold;

    [Header("Shotgun Settings")]
    public float shotgunSpread;
    public int shotgunPellets = 5;

    [Header("UI & Effects")]
    public TextMeshProUGUI currentMagText, backupMagText;
    public Image hudImage;
    public GameObject muzzleFlash, bulletHoleGraphic, bloodDecalPrefab;
    public float camShakeMagnitude, camShakeDuration;

    [Header("References")]
    public GameObject gun;
    public Transform attackPoint;
    public AudioSource gunSound;
    private Transform fpsCam;

    private int bulletsLeft, bulletsShot;
    private bool shooting, readyToShoot, reloading;
    private bool ableToShoot => bulletsLeft > 0 && readyToShoot && !reloading;

    private void Awake()
    {
        fpsCam = Camera.main.transform;
        SetupGunInInventory(false);
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        HandleInput();
        UpdateUI();
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
            bulletsShot = bulletsPerTap;
            Shoot();
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
            ShootBullet();
        }

        bulletsLeft--;
        StartCoroutine(ResetShoot());
    }

    private void ShootBullet()
    {
        Ray ray = new Ray(fpsCam.position, fpsCam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            ApplyHitEffects(hit);
        }
        Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
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
        Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation);
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
        GameObject decal = Instantiate(decalPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        decal.transform.SetParent(hit.transform);
        Destroy(decal, 3f);  // Auto destroy after 3 seconds
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
        hudImage.color = bulletsLeft > 0 ? Color.white : Color.red;
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
}
