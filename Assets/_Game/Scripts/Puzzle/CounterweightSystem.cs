using UnityEngine;

namespace LowAmmo.Puzzle
{
    public class CounterweightSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody2D counterweightRb;
        [SerializeField] private MovingPlatform platform;
        [SerializeField] private LineRenderer ropeLine;
        [SerializeField] private Transform pulleyPoint;
        [SerializeField] private Transform weightRopeAnchor;
        [SerializeField] private Transform platformRopeAnchor;

        [Header("Settings")]
        [SerializeField] private float activationThreshold = 1.0f;

        private Vector3 initialWeightPos;
        private bool hasTriggered = false;

        private void Start()
        {
            if (counterweightRb != null)
            {
                initialWeightPos = counterweightRb.transform.position;
            }
        }

        private void Update()
        {
            UpdateRopeVisual();

            if (!hasTriggered && counterweightRb != null && platform != null)
            {
                if (counterweightRb.bodyType == RigidbodyType2D.Dynamic)
                {
                    float distanceFallen = initialWeightPos.y - counterweightRb.transform.position.y;
                    if (distanceFallen >= activationThreshold)
                    {
                        hasTriggered = true;
                        platform.MoveToDestination();
                    }
                }
            }
        }

        private void UpdateRopeVisual()
        {
            if (ropeLine == null || pulleyPoint == null) return;

            ropeLine.positionCount = 3;
            Vector3 wPos = weightRopeAnchor != null ? weightRopeAnchor.position : (counterweightRb != null ? counterweightRb.transform.position : transform.position);
            Vector3 pPos = platformRopeAnchor != null ? platformRopeAnchor.position : (platform != null ? platform.transform.position : transform.position);

            ropeLine.SetPosition(0, wPos);
            ropeLine.SetPosition(1, pulleyPoint.position);
            ropeLine.SetPosition(2, pPos);
        }

        public void ReleaseCounterweight()
        {
            if (counterweightRb != null)
            {
                counterweightRb.bodyType = RigidbodyType2D.Dynamic;
                counterweightRb.gravityScale = 1f;
            }
            if (platform != null)
            {
                platform.MoveToDestination();
            }
        }
    }
}
