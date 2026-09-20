using UnityEngine;
using LowAmmo.Level;
using LowAmmo.Puzzle;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LowAmmo.Weapon
{
    public class GunController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private SpriteRenderer gunSpriteRenderer;
        [SerializeField] private BulletTracer bulletTracer;

        [Header("Shooting Settings")]
        [SerializeField] private float maxRange = 60f;
        [SerializeField] private LayerMask hitLayers = ~0; // Default hit everything

        [Header("Feedback")]
        [SerializeField] private ParticleSystem muzzleFlashParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip dryFireSound;

        private Camera mainCam;

        private void Awake()
        {
            mainCam = Camera.main;
            if (firePoint == null)
            {
                firePoint = transform;
            }
            if (gunSpriteRenderer == null)
            {
                gunSpriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
                if (mainCam == null) return;
            }

            // Aim toward mouse
            Vector2 mouseScreenPos = Vector2.zero;
            bool firePressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                mouseScreenPos = Mouse.current.position.ReadValue();
                firePressed = Mouse.current.leftButton.wasPressedThisFrame;
            }
#else
            mouseScreenPos = Input.mousePosition;
            firePressed = Input.GetMouseButtonDown(0);
#endif

            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCam.transform.position.z));
            mouseWorldPos.z = 0f;

            Vector2 aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
            if (aimDirection.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);

                if (gunSpriteRenderer != null)
                {
                    // Flip vertically when aiming to the left so the gun stays right-side up
                    gunSpriteRenderer.flipY = Mathf.Abs(angle) > 90f;
                }
            }

            // Shoot interaction
            if (firePressed)
            {
                AttemptFire(aimDirection);
            }
        }

        private void AttemptFire(Vector2 aimDirection)
        {
            if (LevelManager.Instance != null && !LevelManager.Instance.TryConsumeAmmo())
            {
                // Dry fire feedback (out of ammo)
                PlayFeedback(dryFireSound);
                return;
            }

            ExecuteFire(aimDirection);
        }

        private void ExecuteFire(Vector2 aimDirection)
        {
            PlayFeedback(shootSound);
            if (muzzleFlashParticles != null)
            {
                muzzleFlashParticles.Play();
            }

            Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, aimDirection, maxRange, hitLayers);

            RaycastHit2D hit = default;
            bool foundHit = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider != null && (hits[i].collider.CompareTag("Player") || hits[i].collider.transform.IsChildOf(transform.root)))
                {
                    continue; // Skip player's own body
                }
                hit = hits[i];
                foundHit = true;
                break;
            }

            Vector3 hitPoint;
            if (foundHit && hit.collider != null)
            {
                hitPoint = hit.point;

                // Modular IShootable invocation
                IShootable shootable = hit.collider.GetComponentInParent<IShootable>();
                if (shootable == null)
                {
                    shootable = hit.collider.GetComponent<IShootable>();
                }

                if (shootable != null)
                {
                    shootable.OnHit(hit);
                }
            }
            else
            {
                hitPoint = origin + aimDirection * maxRange;
            }

            if (bulletTracer != null)
            {
                bulletTracer.Show(origin, hitPoint);
            }
        }

        private void PlayFeedback(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
