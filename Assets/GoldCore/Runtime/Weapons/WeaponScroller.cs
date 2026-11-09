using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace GoldCore
{
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

        public GameObject CurrentWeapon =>
            weapons != null && weapons.Length > 0 ? weapons[currentWeaponIndex] : null;

        void Start()
        {
            for (int i = 0; i < weapons.Length; i++)
                weapons[i].SetActive(i == currentWeaponIndex);
            UpdateWeaponHUD();
        }

        void Update()
        {
            float scroll = GameInput.WeaponScroll;
            if (scroll == 0f || weapons.Length < 2) return;
            Select((currentWeaponIndex + (scroll > 0f ? 1 : -1) + weapons.Length) % weapons.Length);
        }

        public void Select(int index)
        {
            if (index < 0 || index >= weapons.Length || index == currentWeaponIndex) return;
            weapons[currentWeaponIndex].SetActive(false);
            currentWeaponIndex = index;
            weapons[currentWeaponIndex].SetActive(true);
            UpdateWeaponHUD();
        }

        private void UpdateWeaponHUD()
        {
            GameObject weapon = CurrentWeapon;
            GunSystem gun = weapon != null ? weapon.GetComponent<GunSystem>() : null;
            SetupHUD(gun != null);
            if (gun == null) return;
            if (hudSettings.currentMagText != null) hudSettings.currentMagText.text = gun.BulletsLeft.ToString();
            if (hudSettings.backupMagText != null) hudSettings.backupMagText.text = gun.magazineBackup.ToString();
        }

        private void SetupHUD(bool showHUD)
        {
            if (hudSettings.currentMagText != null) hudSettings.currentMagText.enabled = showHUD;
            if (hudSettings.backupMagText != null) hudSettings.backupMagText.enabled = showHUD;
            if (hudSettings.hudImage != null) hudSettings.hudImage.enabled = showHUD;
        }
    }
}
