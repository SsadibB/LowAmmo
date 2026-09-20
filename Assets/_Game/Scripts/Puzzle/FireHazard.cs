using UnityEngine;
using UnityEngine.Events;
using LowAmmo.Player;

namespace LowAmmo.Puzzle
{
    [RequireComponent(typeof(Collider2D))]
    public class FireHazard : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject fireVisual;
        [SerializeField] private ParticleSystem fireParticles;
        [SerializeField] private ParticleSystem steamEffect;
        [SerializeField] private AudioClip extinguishSound;

        [Header("Events")]
        public UnityEvent onExtinguished;

        private bool isExtinguished = false;
        private Collider2D hazardCollider;

        public bool IsExtinguished => isExtinguished;

        private void Awake()
        {
            hazardCollider = GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandlePlayerContact(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            HandlePlayerContact(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandlePlayerContact(collision.gameObject);
        }

        private void HandlePlayerContact(GameObject obj)
        {
            if (isExtinguished) return;

            var playerDeath = obj.GetComponentInParent<PlayerDeath>();
            if (playerDeath == null) playerDeath = obj.GetComponent<PlayerDeath>();

            if (playerDeath != null)
            {
                playerDeath.Kill();
            }
        }

        public void Extinguish()
        {
            if (isExtinguished) return;
            isExtinguished = true;

            if (hazardCollider != null) hazardCollider.enabled = false;
            if (fireVisual != null) fireVisual.SetActive(false);
            if (fireParticles != null) fireParticles.Stop();

            if (steamEffect != null)
            {
                steamEffect.Play();
            }

            if (extinguishSound != null)
            {
                AudioSource.PlayClipAtPoint(extinguishSound, transform.position);
            }

            onExtinguished?.Invoke();
        }
    }
}
