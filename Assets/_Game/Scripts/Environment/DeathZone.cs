using UnityEngine;
using LowAmmo.Player;

namespace LowAmmo.Environment
{
    [RequireComponent(typeof(Collider2D))]
    public class DeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleDeath(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            HandleDeath(other.gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleDeath(collision.gameObject);
        }

        private void HandleDeath(GameObject obj)
        {
            var playerDeath = obj.GetComponentInParent<PlayerDeath>();
            if (playerDeath == null)
            {
                playerDeath = obj.GetComponent<PlayerDeath>();
            }

            if (playerDeath != null)
            {
                playerDeath.Kill();
            }
        }
    }
}
