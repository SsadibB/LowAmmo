using UnityEngine;
using LowAmmo.Level;

namespace LowAmmo.Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDeath : MonoBehaviour
    {
        [SerializeField] private float restartDelay = 1.0f;
        private PlayerController playerController;
        private bool hasDied;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
        }

        public void Kill()
        {
            if (hasDied) return;
            hasDied = true;

            playerController.Die();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.TriggerPlayerDeath();
            }

            Invoke(nameof(AutoRestart), restartDelay);
        }

        private void AutoRestart()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
        }
    }
}
