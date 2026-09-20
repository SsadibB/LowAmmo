using UnityEngine;
using LowAmmo.Level;

namespace LowAmmo.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool requiresCondition = false;
        [SerializeField] private bool isUnlocked = true;
        [SerializeField] private SpriteRenderer exitRenderer;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color unlockedColor = new Color(0.2f, 0.9f, 0.3f, 0.9f);
        [SerializeField] private AudioClip completeSound;

        public bool IsUnlocked => isUnlocked;

        private void Start()
        {
            if (requiresCondition)
            {
                isUnlocked = false;
            }
            UpdateVisual();
        }

        public void Unlock()
        {
            isUnlocked = true;
            UpdateVisual();
        }

        public void Lock()
        {
            if (requiresCondition)
            {
                isUnlocked = false;
                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            if (exitRenderer != null)
            {
                exitRenderer.color = isUnlocked ? unlockedColor : lockedColor;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isUnlocked) return;

            if (other.CompareTag("Player"))
            {
                if (completeSound != null)
                {
                    AudioSource.PlayClipAtPoint(completeSound, transform.position);
                }

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CompleteLevel();
                }
            }
        }
    }
}
