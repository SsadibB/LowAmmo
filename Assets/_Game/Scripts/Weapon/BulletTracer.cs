using System.Collections;
using UnityEngine;

namespace LowAmmo.Weapon
{
    [RequireComponent(typeof(LineRenderer))]
    public class BulletTracer : MonoBehaviour
    {
        [SerializeField] private float duration = 0.08f;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        public void Show(Vector3 start, Vector3 end)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateTracer(start, end));
        }

        private IEnumerator AnimateTracer(Vector3 start, Vector3 end)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            yield return new WaitForSeconds(duration);

            lineRenderer.enabled = false;
        }
    }
}
