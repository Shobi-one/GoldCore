using UnityEngine;

namespace GoldCore
{
    public class MeleeWeapon : MonoBehaviour
    {
        public int damage;
        public float attackRange;
        public float attackRate;
        [Tooltip("Mark used on surfaces that have no SurfaceTag.")]
        public GameObject bulletHolePrefab;
        public GameObject bloodDecalPrefab;
        public Transform fpsCam;
        public float impactForce = 0.1f;

        private float attackTimer;
        private const float DecalOffset = 0.01f;

        [Header("Weapon Sway")]
        public float swayAmount = 0.02f;
        public float swaySmoothness = 5f;

        private Vector3 initialPosition;

        void Start()
        {
            if (fpsCam == null && Camera.main != null) fpsCam = Camera.main.transform;
            initialPosition = transform.localPosition;
        }

        void Update()
        {
            attackTimer -= Time.deltaTime;
            ApplyWeaponSway();

            if (GameInput.AttackHeld && attackTimer <= 0)
            {
                attackTimer = attackRate;
                PerformAttack();
            }
        }

        private void PerformAttack()
        {
            if (!Physics.Raycast(fpsCam.position, fpsCam.forward, out RaycastHit hit, attackRange, ~0, QueryTriggerInteraction.Ignore))
                return;

            var info = new DamageInfo(damage, DamageType.Club, transform.root.gameObject)
            {
                inflictor = gameObject,
                point = hit.point,
                force = fpsCam.forward * damage * impactForce,
            };

            if (Damage.Apply(hit.collider, info))
            {
                if (bloodDecalPrefab == null) return;
                GameObject blood = Instantiate(bloodDecalPrefab, hit.point + hit.normal * DecalOffset, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                blood.transform.SetParent(hit.transform);
                Destroy(blood, 0.1f);
            }
            else
            {
                Impacts.Spawn(hit, bulletHolePrefab, 0f);
            }
        }

        private void ApplyWeaponSway()
        {
            Vector2 look = GameInput.Look;
            Vector3 targetPosition = initialPosition + new Vector3(-look.x * swayAmount, -look.y * swayAmount, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * swaySmoothness);
        }
    }
}
