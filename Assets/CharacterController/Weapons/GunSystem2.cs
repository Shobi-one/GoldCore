using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GunSystem2 : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots, shotgunSpread;
    public int magazineSize, bulletsPerTap = 1;
    public int magazineBackup;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot, magAmount;

    private int ammoCounter;

    //bools 
    bool shooting, readyToShoot, reloading, ableToShoot, ableToReload;
    public bool inInventory;

    //Reference
    public GameObject gun, bloodDecalPrefab;
    public Transform attackPoint;
    private Transform fpsCam;
    public AudioSource gunSound;
    private RaycastHit rayHit;

    //Graphics
    public GameObject muzzleFlash, bulletHoleGraphic;
    public float camShakeMagnitude, camShakeDuration;
    public TextMeshProUGUI currentMag, backUpMag;
    public Image hud;
    private float bulletHoleTime = 3f;

    private void Start()
    {
        fpsCam = Camera.main.transform;
    }

    private void Awake()
    {
        if (inInventory == true)
        {
            gun.SetActive(true);
            hud.enabled = true;
            currentMag.enabled = true;
            backUpMag.enabled = true;
        }
        else
        {
            gun.SetActive(false);
            hud.enabled = false;
            currentMag.enabled = false;
            backUpMag.enabled = false;
        }

        inInventory = false;

        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        MyInput();

        currentMag.SetText(bulletsLeft.ToString());
        backUpMag.SetText(magazineBackup.ToString());
    }

    private void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetButton("Fire1");
        else shooting = Input.GetButtonDown("Fire1");

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft >= 0 && !reloading && magazineBackup != 0)
        {
            ableToReload = true;
            Reload();
        }

        if (bulletsLeft == 0 && !reloading && magazineBackup != 0)
        {
            ableToReload = true;
            Reload();
        }
        else
        {
            ableToReload = false;
        }

        if (bulletsLeft == 0)
        {
            //Todo set color change into function to clean this up
            currentMag.color = Color.red;
            backUpMag.color = Color.red;
            hud.color = Color.red;
            ableToShoot = false;
        }
        else
        {
            currentMag.color = new Color32(251, 218, 85, 255);
            hud.color = new Color32(255, 255, 255, 255);
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;

            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        for (int i = 0; i < bulletsPerTap; i++)
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * shotgunSpread;
            Ray shoot = new Ray(fpsCam.position, randomDirection);
            if (Physics.Raycast(shoot, out RaycastHit rayHit, range))
            {
                EnemyAI enemy = rayHit.transform.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    GameObject bloodDecal = Instantiate(bloodDecalPrefab, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
                    bloodDecal.transform.SetParent(rayHit.transform);
                    StartCoroutine(DestroyBloodDecal(bloodDecal));
                }
                else if (rayHit.collider.tag == "Wall")
                {
                    GameObject bulletHole = Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
                    bulletHole.transform.SetParent(rayHit.transform);
                    StartCoroutine(DestroyBulletHole(bulletHole));
                }
            }
        }

        StartCoroutine(WaitForShots(timeBetweenShots));
    }

    private IEnumerator WaitForShots(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        readyToShoot = true;
    }

    private IEnumerator DestroyBloodDecal(GameObject decal)
    {
        yield return new WaitForSeconds(bulletHoleTime);

        Destroy(decal);
    }

    private IEnumerator DestroyBulletHole(GameObject hole)
    {
        yield return new WaitForSeconds(bulletHoleTime);

        Destroy(hole);
    }

    private void Reload()
    {
        if (magazineBackup > 0)
        {
            reloading = true;
            magazineBackup += bulletsLeft;
            bulletsLeft = 0;

            if (magazineBackup >= magazineSize)
            {
                bulletsLeft = magazineSize;
                magazineBackup -= magazineSize;
            }
            else
            {
                bulletsLeft += magazineBackup;
                magazineBackup = 0;
            }

            readyToShoot = true;
            reloading = false;
        }
    }
}