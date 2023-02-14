using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ObjectPickupManager : MonoBehaviour
{
    public GameObject player;
    public bool HealthPickup;
    public bool ArmorPickup;
    public bool AmmoPickup;
    [Space]
    public int amount;
    [HideInInspector] public GameObject weapons;

    private void OnTriggerEnter(Collider other)
    {
        if (HealthPickup == true && player.GetComponent<HealthManager>().currentHealth!= 100)
        {
            player.GetComponent<HealthManager>().IncreaseHealth(amount);
            Destroy(gameObject);
        }

        if (ArmorPickup == true && player.GetComponent<HealthManager>().currentArmor != 100)
        {
            player.GetComponent<HealthManager>().IncreaseArmor(amount);
            Destroy(gameObject);
        }

        if (AmmoPickup == true)
        {
            weapons.GetComponent<GunSystem>().IncreaseAmmo(amount);
            Destroy(gameObject);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ObjectPickupManager))]
    public class RandomScript_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ObjectPickupManager script = (ObjectPickupManager)target;

            // draw checkbox for the bool
            if (script.AmmoPickup) // if bool is true, show other fields
            {
                script.weapons = EditorGUILayout.ObjectField(script.weapons, typeof(GameObject), true) as GameObject;
            }
        }
    }
#endif
}
