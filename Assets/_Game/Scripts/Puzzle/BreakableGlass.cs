using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    [RequireComponent(typeof(Collider2D))]
    public class BreakableGlass : MonoBehaviour, IShootable
    {
        [Header("Settings")]
        [SerializeField] private GameObject glassVisual;
        [SerializeField] private ParticleSystem shatterParticles;
        [SerializeField] private AudioClip shatterSound;

        [Header("Events")]
        public UnityEvent onBroken;

        private bool isBroken = false;
        private Collider2D col;

        public bool IsBroken => isBroken;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
        }

        public void OnHit(RaycastHit2D hit)
        {
            if (isBroken) return;
            Break();
        }

        public void Break()
        {
            if (isBroken) return;
            isBroken = true;

            if (col != null) col.enabled = false;
            if (glassVisual != null) glassVisual.SetActive(false);

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;

            if (shatterParticles != null)
            {
                shatterParticles.transform.SetParent(null);
                shatterParticles.Play();
                Destroy(shatterParticles.gameObject, 2f);
            }

            if (shatterSound != null)
            {
                AudioSource.PlayClipAtPoint(shatterSound, transform.position);
            }

            onBroken?.Invoke();
        }
    }
}
