using UnityEngine;

namespace GoldCore
{
    public class RocketLauncher : MonoBehaviour
    {
        public GameObject rocketPrefab;
        public int shotsRemaining = 3;
        public float launchForce = 500f;

        void Update()
        {
            if (!GameInput.AttackPressed || shotsRemaining <= 0) return;

            shotsRemaining--;
            GameObject rocket = Instantiate(rocketPrefab, transform.position, transform.rotation);
            if (rocket.TryGetComponent(out Rocket r)) r.owner = transform.root.gameObject;
            if (rocket.TryGetComponent(out Rigidbody rb)) rb.AddForce(transform.forward * launchForce);
        }
    }
}
