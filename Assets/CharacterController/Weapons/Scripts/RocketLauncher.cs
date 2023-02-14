using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;
    public int shotsRemaining = 3;
    public float launchForce = 500f;

    private GameObject rocket;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && shotsRemaining > 0)
        {
            shotsRemaining--;
            rocket = Instantiate(rocketPrefab, transform.position, transform.rotation);
            Rigidbody rb = rocket.GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * launchForce);
        }
    }
}



