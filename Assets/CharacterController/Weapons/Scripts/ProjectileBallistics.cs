using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBallistics : MonoBehaviour
{
    private Rigidbody rb;
    private float gravity;
    private float drag;
    private float maxLifetime;
    private float currentLifetime = 0f;

    public void SetupBallistics(Rigidbody projectileRb, float gravityValue, float dragValue, float lifetime)
    {
        rb = projectileRb;
        gravity = gravityValue;
        drag = dragValue;
        maxLifetime = lifetime;
    }

    private void FixedUpdate()
    {
        // Apply custom gravity
        rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;

        // Apply drag (simulates air resistance)
        rb.linearVelocity -= rb.linearVelocity * drag * Time.fixedDeltaTime;

        // Increase lifetime and destroy projectile after max lifetime
        currentLifetime += Time.fixedDeltaTime;
        if (currentLifetime >= maxLifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Handle hit and apply damage, etc.
        Destroy(gameObject);  // Destroy the projectile on impact
    }
}
