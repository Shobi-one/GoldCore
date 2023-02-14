using UnityEngine;

public class Button : MonoBehaviour
{
    [SerializeField] public float posX, posY, posZ;
    [SerializeField] public Transform spawnLocation;
    public GameObject objectToCreate;
    public float pressDistance = 2.0f;
    public Transform playerTransform;
    public Transform location;
    public bool moreObjects;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (Physics.Raycast(playerTransform.position, playerTransform.forward, out hit, pressDistance))
            {
                Debug.DrawLine(playerTransform.position, hit.point, Color.red);

                if (hit.collider.gameObject == this.gameObject && moreObjects == false)
                {
                    Instantiate(objectToCreate, location.position, transform.rotation);
                }
                else if (hit.collider.gameObject == this.gameObject && moreObjects == false)
                {
                    Instantiate(gameObject, location.position, spawnLocation.rotation);
                }
            }
        }
    }
}
