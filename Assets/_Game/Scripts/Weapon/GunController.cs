using UnityEngine;
using LowAmmo.Level;
using LowAmmo.Puzzle;
using UnityEngine.InputSystem;

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
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Feedback")]
        [SerializeField] private ParticleSystem muzzleFlashParticles;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip dryFireSound;

        private Camera mainCam;
        private int groundLayer;

        private void Awake()
        {
            mainCam = Camera.main;
            groundLayer = LayerMask.NameToLayer("Ground");
            if (firePoint == null) firePoint = transform;
            if (gunSpriteRenderer == null) gunSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }

            if (LevelManager.Instance != null && (LevelManager.Instance.IsGameOver || LevelManager.Instance.IsLevelCompleted))
            {
                return;
            }

            Vector2 mouseScreenPos = ReadPointerScreenPosition();
            bool firePressed = ReadFirePressed();

            Vector3 mouseWorldPos = transform.position + Vector3.right;
            if (mainCam != null)
            {
                mouseWorldPos = mainCam.ScreenToWorldPoint(
                    new Vector3(mouseScreenPos.x, mouseScreenPos.y, Mathf.Abs(mainCam.transform.position.z)));
                mouseWorldPos.z = 0f;
            }

            Vector2 aimDirection = ((Vector2)mouseWorldPos - (Vector2)transform.position);
            if (aimDirection.sqrMagnitude < 0.0001f)
            {
                aimDirection = Vector2.right;
            }
            else
            {
                aimDirection.Normalize();
            }

            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (gunSpriteRenderer != null)
            {
                gunSpriteRenderer.flipY = Mathf.Abs(angle) > 90f;
            }

            if (firePressed)
            {
                AttemptFire(aimDirection);
            }
        }

        // New Input System only. Pointer.current covers both mouse and touch;
        // the legacy UnityEngine.Input calls were removed because they throw
        // InvalidOperationException when Active Input Handling is "Input System Package (New)".
        private static Vector2 ReadPointerScreenPosition()
        {
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            if (Pointer.current != null)
                return Pointer.current.position.ReadValue();
            if (Touchscreen.current != null)
                return Touchscreen.current.primaryTouch.position.ReadValue();
            return Vector2.zero;
        }

        private static bool ReadFirePressed()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                return true;

            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
                return true;

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;

            return false;
        }

        private void AttemptFire(Vector2 aimDirection)
        {
            if (LevelManager.Instance != null && !LevelManager.Instance.TryConsumeAmmo())
            {
                PlayFeedback(dryFireSound);
                ExecuteFire(aimDirection, consumeSucceeded: false);
                return;
            }

            ExecuteFire(aimDirection, consumeSucceeded: true);
        }

        private void ExecuteFire(Vector2 aimDirection, bool consumeSucceeded)
        {
            if (consumeSucceeded)
            {
                PlayFeedback(shootSound);
                if (muzzleFlashParticles != null) muzzleFlashParticles.Play();
            }

            Vector2 origin = firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, aimDirection, maxRange, hitLayers);

            RaycastHit2D hit = default;
            bool foundHit = false;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider == null) continue;
                if (hits[i].collider.CompareTag("Player")) continue;
                if (hits[i].collider.transform.IsChildOf(transform.root)) continue;

                IShootable shootable = hits[i].collider.GetComponentInParent<IShootable>()
                    ?? hits[i].collider.GetComponent<IShootable>();
                if (shootable != null)
                {
                    hit = hits[i];
                    foundHit = true;
                    break;
                }

                if (hits[i].collider.isTrigger) continue;

                // Crate colliders sit in front of ropes; only solid ground/doors should stop a shot.
                if (hits[i].collider.gameObject.layer == groundLayer)
                {
                    hit = hits[i];
                    foundHit = true;
                    break;
                }
            }

            Vector3 hitPoint = foundHit && hit.collider != null ? (Vector3)hit.point : origin + aimDirection * maxRange;

            if (consumeSucceeded && foundHit && hit.collider != null)
            {
                IShootable shootable = hit.collider.GetComponentInParent<IShootable>()
                    ?? hit.collider.GetComponent<IShootable>();
                if (shootable != null) shootable.OnHit(hit);
            }

            if (bulletTracer != null) bulletTracer.Show(origin, hitPoint);
        }

        private void PlayFeedback(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
    }
}