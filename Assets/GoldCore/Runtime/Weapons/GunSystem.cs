using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GoldCore
{
    [RequireComponent(typeof(AudioSource))]
    public class GunSystem : MonoBehaviour
    {
        public enum GunType { Pistol, Shotgun, Rifle }
        public enum FireMode { Hitscan, Projectile }

        [Header("Gun Settings")]
        public GunType gunType;
        public FireMode fireMode = FireMode.Hitscan;
        public bool autoReload = true;
        public int damage;
        public float timeBetweenShooting, range, reloadTime, timeBetweenShots;
        public int magazineSize, bulletsPerTap = 1;
        public int magazineBackup;
        public bool allowButtonHold;
        public bool isBurstFire;
        public int burstCount = 3;
        public float bulletSpread = 0.05f;
        [Tooltip("Push given to rigidbodies that are hit, per point of damage.")]
        public float impactForce = 0.05f;

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
        // bulletHoleGraphic is used on surfaces that have no SurfaceTag.
        public GameObject muzzleFlash, bulletHoleGraphic, bloodDecalPrefab;
        public float camShakeMagnitude, camShakeDuration;

        [Header("References")]
        public GameObject gun;
        public Transform attackPoint;
        public AudioSource gunSound;

        [SerializeField] private Transform fpsCam;

        private int bulletsLeft;
        private bool readyToShoot, reloading;
        private bool AbleToShoot => bulletsLeft > 0 && readyToShoot && !reloading;

        public int BulletsLeft => bulletsLeft;
        public bool IsReloading => reloading;

        [Header("Weapon Sway Settings")]
        public float swayAmount = 0.02f;
        public float swaySmoothness = 5f;
        public float swayRotationAmount = 4f;

        private Vector3 initialGunPosition;

        private void Awake()
        {
            if (fpsCam == null && Camera.main != null) fpsCam = Camera.main.transform;
            SetupGunInInventory(false);
            bulletsLeft = magazineSize;
            readyToShoot = true;
            initialGunPosition = gun.transform.localPosition;
        }

        private void OnDisable()
        {
            // Switching weapons mid-reload or mid-shot must not leave the gun stuck.
            StopAllCoroutines();
            reloading = false;
            readyToShoot = true;
        }

        private void Update()
        {
            HandleInput();
            UpdateUI();
            ApplyWeaponSway();

            if (autoReload && bulletsLeft == 0 && !reloading && magazineBackup > 0)
                StartCoroutine(Reload());
        }

        private void HandleInput()
        {
            bool shooting = allowButtonHold ? GameInput.AttackHeld : GameInput.AttackPressed;

            if (GameInput.ReloadPressed && bulletsLeft < magazineSize && !reloading && magazineBackup > 0)
                StartCoroutine(Reload());

            if (AbleToShoot && shooting)
            {
                if (isBurstFire) StartCoroutine(BurstFire());
                else Shoot();
            }
        }

        private IEnumerator BurstFire()
        {
            for (int i = 0; i < burstCount; i++)
            {
                if (!AbleToShoot) break;
                Shoot();
                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        private void Shoot()
        {
            readyToShoot = false;
            if (gunSound != null) gunSound.Play();

            if (gunType == GunType.Shotgun)
            {
                for (int i = 0; i < shotgunPellets; i++)
                    FireHitscan(fpsCam.forward + Random.insideUnitSphere * shotgunSpread);
            }
            else if (fireMode == FireMode.Projectile)
            {
                FireProjectile();
            }
            else
            {
                FireHitscan(ApplySpread(fpsCam.forward, bulletSpread));
            }

            SpawnMuzzleFlash();
            bulletsLeft--;
            StartCoroutine(ResetShoot());
        }

        private void FireHitscan(Vector3 direction)
        {
            direction.Normalize();
            if (Physics.Raycast(fpsCam.position, direction, out RaycastHit hit, range, ~0, QueryTriggerInteraction.Ignore))
                ApplyHitEffects(hit, direction);
        }

        private void FireProjectile()
        {
            GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, attackPoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb == null) return;

            rb.linearVelocity = ApplySpread(fpsCam.forward, bulletSpread) * projectileInitialSpeed;
            rb.useGravity = false;
            ProjectileBallistics ballistics = projectile.AddComponent<ProjectileBallistics>();
            ballistics.SetupBallistics(rb, projectileGravity, projectileDrag, maxLifetime);
            ballistics.SetDamage(damage, transform.root.gameObject, bulletHoleGraphic);
        }

        private void SpawnMuzzleFlash()
        {
            if (muzzleFlash == null || attackPoint == null) return;
            Destroy(Instantiate(muzzleFlash, attackPoint.position, attackPoint.rotation), 0.05f);
        }

        private static Vector3 ApplySpread(Vector3 direction, float spread) =>
            (direction + Random.insideUnitSphere * spread).normalized;

        private void ApplyHitEffects(RaycastHit hit, Vector3 direction)
        {
            var info = new DamageInfo(damage, DamageType.Bullet, transform.root.gameObject)
            {
                inflictor = gameObject,
                point = hit.point,
                force = direction * damage * impactForce,
            };

            if (Damage.Apply(hit.collider, info))
                CreateBloodDecal(hit);
            else
                Impacts.Spawn(hit, bulletHoleGraphic, 3f);
        }

        private void CreateBloodDecal(RaycastHit hit)
        {
            if (bloodDecalPrefab == null) return;
            GameObject decal = Instantiate(bloodDecalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
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
            Color magColor = bulletsLeft > 0 ? new Color32(251, 218, 85, 255) : Color.red;
            if (currentMagText != null)
            {
                currentMagText.SetText(bulletsLeft.ToString());
                currentMagText.color = magColor;
            }
            if (backupMagText != null)
            {
                backupMagText.SetText(magazineBackup.ToString());
                backupMagText.color = magColor;
            }
            if (hudImage != null) hudImage.color = bulletsLeft > 0 ? Color.white : Color.red;
        }

        private void SetupGunInInventory(bool inInventory)
        {
            gun.SetActive(inInventory);
            if (hudImage != null) hudImage.enabled = inInventory;
            if (currentMagText != null) currentMagText.enabled = inInventory;
            if (backupMagText != null) backupMagText.enabled = inInventory;
        }

        public void IncreaseAmmo(int amount)
        {
            magazineBackup += amount;
        }

        private void ApplyWeaponSway()
        {
            Vector2 look = GameInput.Look;
            Vector3 targetPosition = initialGunPosition + new Vector3(-look.x * swayAmount, -look.y * swayAmount, 0);
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
        }
    }
}
