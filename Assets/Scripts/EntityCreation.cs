using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntityCreation : MonoBehaviour
{
    [SerializeField] public float posX, posY, posZ;
    [SerializeField] public GameObject gameObject;
    [SerializeField] public Transform spawnLocation;
    public int gameObjectAmount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButtonDown("Use"))
        {
            Instantiate(gameObject, spawnLocation.position, spawnLocation.rotation);
        }
    }
}
