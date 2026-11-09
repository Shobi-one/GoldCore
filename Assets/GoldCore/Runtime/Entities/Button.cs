using UnityEngine;

namespace GoldCore
{
    /// <summary>
    /// Spawns a prefab when the player looks at it and presses use. Becomes func_button with outputs
    /// in the entities phase.
    /// </summary>
    public class Button : MonoBehaviour
    {
        [Tooltip("Where the object appears when More Objects is on.")]
        public Transform spawnLocation;
        public GameObject objectToCreate;
        public float pressDistance = 2.0f;
        [Tooltip("The player's camera, used to aim the press.")]
        public Transform playerTransform;
        [Tooltip("Where the object appears.")]
        public Transform location;
        [Tooltip("Spawn at Spawn Location with its rotation instead of at Location with the button's rotation.")]
        public bool moreObjects;

        void Update()
        {
            if (!GameInput.UsePressed || playerTransform == null || objectToCreate == null) return;
            if (!Physics.Raycast(playerTransform.position, playerTransform.forward, out RaycastHit hit, pressDistance)) return;
            if (hit.collider.gameObject != gameObject) return;

            if (moreObjects && spawnLocation != null)
                Instantiate(objectToCreate, spawnLocation.position, spawnLocation.rotation);
            else if (location != null)
                Instantiate(objectToCreate, location.position, transform.rotation);
        }
    }
}
