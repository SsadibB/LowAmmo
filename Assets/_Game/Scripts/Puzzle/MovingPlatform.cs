using UnityEngine;

namespace LowAmmo.Puzzle
{
    public class MovingPlatform : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private bool autoMove = false;

        private Vector3 targetPos;
        private bool isMoving = false;

        private void Awake()
        {
            if (startPoint == null)
            {
                var startObj = new GameObject("StartPoint");
                startObj.transform.position = transform.position;
                startPoint = startObj.transform;
            }
            targetPos = startPoint.position;
            if (autoMove && endPoint != null)
            {
                targetPos = endPoint.position;
                isMoving = true;
            }
        }

        private void Update()
        {
            if (!isMoving) return;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                if (autoMove && endPoint != null && startPoint != null)
                {
                    targetPos = (targetPos == startPoint.position) ? endPoint.position : startPoint.position;
                }
                else
                {
                    isMoving = false;
                }
            }
        }

        public void MoveToDestination()
        {
            if (endPoint != null)
            {
                targetPos = endPoint.position;
                isMoving = true;
            }
        }

        public void MoveToStart()
        {
            if (startPoint != null)
            {
                targetPos = startPoint.position;
                isMoving = true;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.transform.SetParent(null);
            }
        }
    }
}
