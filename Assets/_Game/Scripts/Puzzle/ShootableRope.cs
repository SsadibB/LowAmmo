using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    public class ShootableRope : MonoBehaviour, IShootable
    {
        [Header("Connected Objects")]
        [SerializeField] private Rigidbody2D connectedBody;
        [SerializeField] private Joint2D connectedJoint;

        [Header("Settings")]
        [SerializeField] private bool disableRopeOnCut = true;
        [SerializeField] private GameObject ropeVisual;
        [SerializeField] private ParticleSystem cutEffect;
        [SerializeField] private AudioClip cutSound;

        [Header("Events")]
        public UnityEvent onRopeCut;

        private bool isCut = false;
        public bool IsCut => isCut;

        public void OnHit(RaycastHit2D hit)
        {
            if (isCut) return;
            CutRope();
        }

        public void CutRope()
        {
            if (isCut) return;
            isCut = true;

            if (connectedJoint != null)
            {
                Destroy(connectedJoint);
            }

            if (connectedBody != null)
            {
                connectedBody.bodyType = RigidbodyType2D.Dynamic;
                connectedBody.gravityScale = 1f;
                connectedBody.constraints = RigidbodyConstraints2D.None;
                connectedBody.WakeUp();
            }

            if (cutEffect != null)
            {
                cutEffect.transform.SetParent(null);
                cutEffect.Play();
                Destroy(cutEffect.gameObject, 2f);
            }

            if (cutSound != null)
            {
                AudioSource.PlayClipAtPoint(cutSound, transform.position);
            }

            if (disableRopeOnCut)
            {
                if (ropeVisual != null) ropeVisual.SetActive(false);
                var col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                var sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = false;
            }

            onRopeCut?.Invoke();
        }
    }
}
