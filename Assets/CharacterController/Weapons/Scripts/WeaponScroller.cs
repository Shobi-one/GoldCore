using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponScroller : MonoBehaviour
{
    [System.Serializable]
    public class HUDSettings
    {
        public TextMeshProUGUI currentMagText;
        public TextMeshProUGUI backupMagText;
        public Image hudImage;
    }

    public GameObject[] weapons;
    public HUDSettings hudSettings;
    private int currentWeaponIndex = 0;

    private GunSystem currentGunSystem;
    private MeleeWeapon currentMeleeWeapon;

    void Start()
    {
        InitializeWeapons();
        UpdateWeaponHUD();
    }

    void Update()
    {
        HandleWeaponScroll();
    }

    private void InitializeWeapons()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].SetActive(i == currentWeaponIndex);
        }
    }

    private void HandleWeaponScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int direction = scroll > 0f ? 1 : -1;
            int previousIndex = currentWeaponIndex;
            currentWeaponIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;

            weapons[previousIndex].SetActive(false);
            weapons[currentWeaponIndex].SetActive(true);

            UpdateWeaponHUD();
        }
    }

    private void UpdateWeaponHUD()
    {
        // Check if the new weapon has a GunSystem or MeleeWeapon component
        currentGunSystem = weapons[currentWeaponIndex].GetComponent<GunSystem>();
        currentMeleeWeapon = weapons[currentWeaponIndex].GetComponent<MeleeWeapon>();

        if (currentGunSystem != null)
        {
            SetupHUD(true);
            hudSettings.currentMagText.text = currentGunSystem.magazineSize.ToString();
            hudSettings.backupMagText.text = currentGunSystem.magazineBackup.ToString();
        }
        else if (currentMeleeWeapon != null)
        {
            SetupHUD(false); // Hide HUD for melee weapons if desired
        }
        else
        {
            SetupHUD(false); // Hide HUD if no weapon component
        }
    }

    private void SetupHUD(bool showHUD)
    {
        if (showHUD)
        {
            hudSettings.currentMagText.enabled = true;
            hudSettings.backupMagText.enabled = true;
            hudSettings.hudImage.enabled = true;
        }
        else
        {
            hudSettings.currentMagText.enabled = false;
            hudSettings.backupMagText.enabled = false;
            hudSettings.hudImage.enabled = false;
        }
    }
}
