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

    void Start()
    {
        PlayerAiming = cameraHolder.GetComponent<PlayerAiming>();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (CurrentObject || playerIsStandingOnObject)
            {
                CurrentObject.velocity = Vector3.zero;
                CurrentObject.angularVelocity = Vector3.zero;
                CurrentObject.freezeRotation = false;
                CurrentObject.useGravity = true;
                CurrentObject = null;
                CurrentObjectTransform = null;
                weaponHolder.SetActive(true);
                return;
            }

            Ray CameraRay = PlayerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(CameraRay, out RaycastHit HitInfo, PickupRange, PickupMask))
            {
                CurrentObjectTransform = HitInfo.transform;
                CurrentObject = HitInfo.rigidbody;
                PickupTarget.rotation = CurrentObject.rotation;
                //CurrentObject.freezeRotation = true;
                CurrentObject.useGravity = false;
                weaponHolder.SetActive(false);
                Physics.IgnoreLayerCollision(6, 3);
            }
        }
        if (CurrentObjectTransform != null)
        {
            //Rotate();
        }

        if (Input.GetMouseButtonDown(0) && CurrentObject)
        {
            CurrentObject.AddForce(PlayerCam.transform.forward * throwForce, ForceMode.Impulse);
            CurrentObject.velocity = Vector3.zero;
            CurrentObject.angularVelocity = Vector3.zero;
            CurrentObject.freezeRotation = false;
            CurrentObject.useGravity = true;
            CurrentObject = null;
            CurrentObjectTransform = null;
            weaponHolder.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (CurrentObject)
        {
            Vector3 DirectionToPoint = PickupTarget.position - CurrentObject.position;
            float DistanceToPoint = DirectionToPoint.magnitude;
            CurrentObject.velocity = DirectionToPoint * 12f * DistanceToPoint;
            AlignToPlayer();
        }
    }

    public void AlignToPlayer()
    {
        Vector3 CameraToPlayer = new Vector3();
        CameraToPlayer = PlayerCam.transform.position - PickupTarget.position;
        Quaternion TargetRotation = PickupTarget.rotation;
        Quaternion MoveToRotation = Quaternion.RotateTowards(CurrentObject.rotation, TargetRotation, alignedSpeed * Time.fixedDeltaTime);
        CurrentObject.MoveRotation(MoveToRotation);
        Debug.Log(CameraToPlayer);
    }


    //Scrapped due to it being unneeded. Might use it for later idk.
    //public void Rotate()
    //{
    //    if (Input.GetMouseButton(1))
    //    {
    //        PlayerAiming.horizontalSensitivity = 0f;
    //        PlayerAiming.verticalSensitivity = 0f;
    //        PickupTarget.Rotate(new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0) * Time.deltaTime * rotSpeed);
    //        AlignToPlayer();
    //    }
    //    else
    //    {
    //        PlayerAiming.horizontalSensitivity = 1f;
    //        PlayerAiming.verticalSensitivity = 1f;
    //    }
    //}

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