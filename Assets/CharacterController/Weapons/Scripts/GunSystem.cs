using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
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
    public Transform fpsCam;
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
            backUpMag.color = new Color32(251, 218, 85, 255);
            hud.color = new Color32(255, 255, 255, 255);
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0){
            bulletsShot = bulletsPerTap;

            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;

        Ray shoot = new Ray(fpsCam.position, fpsCam.forward);
        if (Physics.Raycast(shoot, out RaycastHit rayHit, range))
        {
            EnemyAI enemy = rayHit.collider.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                GameObject bloodDecal = Instantiate(bloodDecalPrefab, rayHit.point, Quaternion.FromToRotation(Vector3.forward, rayHit.normal));
                bloodDecal.transform.SetParent(rayHit.transform);
                StartCoroutine(DestroyBloodDecal(bloodDecal));
            }
            else if (rayHit.collider.tag == "Wall")
            {
                GameObject bulletHole = Instantiate(bulletHoleGraphic, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                bulletHole.transform.parent = rayHit.collider.transform;
            }
        }


        //Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);

        bulletsLeft--;
        bulletsShot--;
        gunSound.Play();

        Invoke("ResetShot", timeBetweenShooting);
        Camera.main.transform.DOKill();

        if (bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", timeBetweenShots);
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    private void Reload()
    {
        //honestly this part of the code was the bane of my existance...
        //I know there is definitaly a better way to go about this BUT fuck this shit man this is how i'm doing it.

        if(ableToReload == true)
        {
            reloading = true;
            Invoke("ReloadFinished", reloadTime);
            ammoCounter = magazineSize - bulletsLeft;
            magazineBackup = magazineBackup - ammoCounter;

            if (magazineBackup < 0)
            {
                ammoCounter = magazineBackup;
                magazineBackup = 0;
            }
        }
    }

    private void ReloadFinished()
    {
        if (ammoCounter > 0)
        {
            bulletsLeft = magazineSize;
        } 
        else if (ammoCounter < 0)
        {
            bulletsLeft = magazineSize + ammoCounter;
        }
        reloading = false;
    }

    private void UnableToShoot()
    {
        ableToShoot = false;
        if(ableToShoot == false)
        {
            bulletsLeft = 0;
            ableToReload = false;
        }
        else
        {
            bulletsLeft = magazineSize;
            ableToReload = true;
        }
    }
    IEnumerator DestroyBloodDecal(GameObject bloodDecal)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(bloodDecal);
    }

    public void IncreaseAmmo(int amount)
    {
        magazineBackup += amount;
    }
}
