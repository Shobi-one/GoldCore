using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupScript : MonoBehaviour
{
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private GameObject playerGameObject;
    [SerializeField] private LayerMask PickupMask;
    [SerializeField] private Camera PlayerCam;
    [SerializeField] private Transform PickupTarget;
    [SerializeField] private GameObject weaponHolder;
    [Space]
    [SerializeField] private float PickupRange;
    [SerializeField] private float throwForce;
    private Rigidbody CurrentObject;
    private Transform CurrentObjectTransform;
    private float rotSpeed = 100f;
    private float alignedSpeed = 3000f;
    private bool playerIsStandingOnObject;
    Quaternion PickedupRotation;
    PlayerAiming PlayerAiming;

    public float springConstant = 525f; // Default spring constant
    private float defaultSpringConstant = 525f; // Store the default spring constant
    private float dampingFactor = 0.5f;  // Damping factor for object movement

    void Start()
    {
        PlayerAiming = cameraHolder.GetComponent<PlayerAiming>();
    }

    void Update()
    {
        // Handling pick up and throw
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CurrentObject || playerIsStandingOnObject)
            {
                // Drop the object
                ReleaseObject();
                return;
            }

            // Pickup logic
            Ray CameraRay = PlayerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(CameraRay, out RaycastHit HitInfo, PickupRange, PickupMask))
            {
                CurrentObjectTransform = HitInfo.transform;
                CurrentObject = HitInfo.rigidbody;

                if (CurrentObject != null)
                {
                    PickupTarget.rotation = CurrentObject.rotation;
                    CurrentObject.useGravity = false;
                    weaponHolder.SetActive(false);
                    Physics.IgnoreLayerCollision(6, 3); // Adjust layer if necessary
                }
            }
        }

        // Throw logic
        if (Input.GetMouseButtonDown(0) && CurrentObject)
        {
            ThrowObject();
        }
    }

    void ThrowObject()
    {
        if (CurrentObject)
        {
            // Add force based on the object's mass to account for the impact force correctly
            float massFactor = CurrentObject.mass;
            Vector3 throwDirection = PlayerCam.transform.forward;

            CurrentObject.AddForce(throwDirection * throwForce * massFactor, ForceMode.Impulse);

            // Reset the object's physics properties
            ReleaseObject();
        }
    }

    void ReleaseObject()
    {
        // Reset object properties when releasing or dropping the object
        if (CurrentObject)
        {
            CurrentObject.velocity = Vector3.zero;
            CurrentObject.angularVelocity = Vector3.zero;
            CurrentObject.freezeRotation = false;
            CurrentObject.useGravity = true;

            CurrentObject = null;
            CurrentObjectTransform = null;
            weaponHolder.SetActive(true);

            // Reset the spring constant to default
            springConstant = defaultSpringConstant;
        }
    }

    void FixedUpdate()
    {
        if (CurrentObject)
        {
            // Calculate the direction and distance to the target position
            Vector3 DirectionToPoint = PickupTarget.position - CurrentObject.position;
            float DistanceToPoint = DirectionToPoint.magnitude;

            // Apply a spring-like force towards the target
            Vector3 forceToApply = DirectionToPoint * springConstant * Time.fixedDeltaTime;

            // Add the force and apply damping to the velocity to avoid oscillation
            CurrentObject.velocity = (CurrentObject.velocity + forceToApply) * dampingFactor;

            // Apply a limit to prevent too much velocity
            CurrentObject.velocity = Vector3.ClampMagnitude(CurrentObject.velocity, 100f); // Adjust max speed

            // Align rotation smoothly
            AlignToPlayer();
        }
    }

    public void AlignToPlayer()
    {
        // Smoothly rotate the object to align with the target’s rotation
        Quaternion TargetRotation = PickupTarget.rotation;
        Quaternion MoveToRotation = Quaternion.RotateTowards(CurrentObject.rotation, TargetRotation, alignedSpeed * Time.fixedDeltaTime);
        CurrentObject.MoveRotation(MoveToRotation);
    }

    // Handle collision with another object to adjust spring constant
    private void OnCollisionEnter(Collision collision)
    {
        if (CurrentObject && collision.rigidbody)
        {
            float otherMass = collision.rigidbody.mass;
            Debug.Log(collision.rigidbody.mass);
            springConstant = defaultSpringConstant - (otherMass * 50f);

            // Ensure spring constant doesn't go negative
            if (springConstant < 0)
            {
                springConstant = 0;
            }
        }
    }

    // Reset spring constant when no longer colliding
    private void OnCollisionExit(Collision collision)
    {
        if (CurrentObject && collision.rigidbody)
        {
            // Reset to default spring constant
            springConstant = defaultSpringConstant;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerGameObject)
        {
            playerIsStandingOnObject = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == playerGameObject)
        {
            playerIsStandingOnObject = false;
        }
    }
}