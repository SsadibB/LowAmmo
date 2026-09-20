using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    public class WaterSource : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject waterFlowVisual;
        [SerializeField] private Collider2D waterCollider;
        [SerializeField] private ParticleSystem waterParticles;
        [SerializeField] private FireHazard targetFireHazard;
        [SerializeField] private float extinguishDelay = 0.4f;

        [Header("Events")]
        public UnityEvent onFlowStarted;
        public UnityEvent onFlowStopped;

        private bool isFlowing = false;
        public bool IsFlowing => isFlowing;

        private void Awake()
        {
            if (waterFlowVisual != null) waterFlowVisual.SetActive(false);
            if (waterCollider != null) waterCollider.enabled = false;
        }

        public void StartWaterFlow()
        {
            if (isFlowing) return;
            isFlowing = true;

            if (waterFlowVisual != null) waterFlowVisual.SetActive(true);
            if (waterCollider != null) waterCollider.enabled = true;
            if (waterParticles != null) waterParticles.Play();

            if (targetFireHazard != null)
            {
                StartCoroutine(ExtinguishTargetCoroutine());
            }

            onFlowStarted?.Invoke();
        }

        public void StopWaterFlow()
        {
            if (!isFlowing) return;
            isFlowing = false;

            if (waterFlowVisual != null) waterFlowVisual.SetActive(false);
            if (waterCollider != null) waterCollider.enabled = false;
            if (waterParticles != null) waterParticles.Stop();

            onFlowStopped?.Invoke();
        }

        private IEnumerator ExtinguishTargetCoroutine()
        {
            yield return new WaitForSeconds(extinguishDelay);
            if (targetFireHazard != null)
            {
                targetFireHazard.Extinguish();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isFlowing) return;

            var fire = other.GetComponent<FireHazard>();
            if (fire != null)
            {
                fire.Extinguish();
            }
        }
    }
}
