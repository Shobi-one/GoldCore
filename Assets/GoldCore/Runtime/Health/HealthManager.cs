using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GoldCore
{
    /// <summary>
    /// The player's <see cref="Health"/>, plus the health and armor numbers on the HUD.
    /// The HUD fields are optional. They move to a separate HUD prefab in the weapons phase.
    /// </summary>
    public class HealthManager : Health
    {
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI armorText;
        public Image hud;

        [Tooltip("Health at or below this, with no armor, turns the HUD red.")]
        public int lowHealthWarning = 30;
        public Color normalColor = new Color32(251, 218, 85, 255);
        public Color warningColor = Color.red;

        void Start() => Changed();

        // Kept so older scripts and UnityEvents keep working.
        public void IncreaseHealth(int amount) => Heal(amount);
        public void IncreaseArmor(int amount) => AddArmor(amount);
        public void DecreaseHealth(int amount) => TakeDamage(new DamageInfo(amount));
        public void DecreaseArmor(int amount)
        {
            currentArmor = Mathf.Max(0, currentArmor - amount);
            Changed();
        }

        protected override void Changed()
        {
            base.Changed();
            bool warning = currentHealth <= lowHealthWarning && currentArmor == 0;
            Color textColor = warning ? warningColor : normalColor;
            if (healthText != null)
            {
                healthText.color = textColor;
                healthText.SetText(currentHealth.ToString());
            }
            if (armorText != null)
            {
                armorText.color = textColor;
                armorText.SetText(currentArmor.ToString());
            }
            if (hud != null) hud.color = warning ? warningColor : Color.white;
        }
    }
}
