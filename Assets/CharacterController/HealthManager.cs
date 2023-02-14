using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthManager : MonoBehaviour
{
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI armorText;
    public Image hud;

    public int currentHealth = 100;
    public int currentArmor = 0;

    void Start()
    {
        UpdateHealthText();
        UpdateArmorText();
    }


    public void IncreaseHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, 100);
        UpdateHealthText();
    }

    public void IncreaseArmor(int amount)
    {
        currentArmor += amount;
        currentArmor = Mathf.Clamp(currentArmor, 0, 100);
        UpdateArmorText();
    }

    public void DecreaseHealth(int amount)
    {
        {
            if (currentArmor > 0)
            {
                DecreaseArmor(amount);
            }
            else
            {
                currentHealth -= amount;
                currentHealth = Mathf.Clamp(currentHealth, 0, 100);
                UpdateHealthText();
            }
        }
    }

    public void DecreaseArmor(int amount)
    {
        currentArmor -= amount;
        currentArmor = Mathf.Clamp(currentArmor, 0, 100);
        UpdateArmorText();
    }

    void UpdateHealthText()
    {
        if (currentHealth <= 30 && currentArmor == 0)
        {
            healthText.color = Color.red;
            armorText.color = Color.red;
            hud.color = Color.red;
        }
        else
        {
            healthText.color = new Color32(251, 218, 85, 255);
            armorText.color = new Color32(251, 218, 85, 255);
            hud.color = new Color32(255, 255, 255, 255);
        }

        healthText.SetText(currentHealth.ToString());

    }

    void UpdateArmorText()
    {
        armorText.SetText(currentArmor.ToString());
    }
}
