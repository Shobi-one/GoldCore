using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScroller : MonoBehaviour
{
    public GameObject[] weapons;
    private int currentWeaponIndex = 0;

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            int direction = scroll > 0f ? 1 : -1;
            currentWeaponIndex = (currentWeaponIndex + direction + weapons.Length) % weapons.Length;

            weapons[currentWeaponIndex].SetActive(true);
            weapons[(currentWeaponIndex - direction + weapons.Length) % weapons.Length].SetActive(false);
        }
    }
}




