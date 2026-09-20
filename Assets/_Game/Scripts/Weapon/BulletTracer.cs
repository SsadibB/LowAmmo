using System.Collections;
using UnityEngine;

namespace LowAmmo.Weapon
{
    [RequireComponent(typeof(LineRenderer))]
    public class BulletTracer : MonoBehaviour
    {
        [SerializeField] private float duration = 0.18f;
        private LineRenderer lineRenderer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            ConfigureRenderer();
        }

        private void ConfigureRenderer()
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 50;
            lineRenderer.numCapVertices = 4;
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.03f;
            lineRenderer.startColor = new Color(1f, 0.95f, 0.35f, 1f);
            lineRenderer.endColor = new Color(1f, 0.45f, 0.1f, 0.9f);

            if (lineRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader != null)
                    lineRenderer.material = new Material(shader);
            }

            lineRenderer.enabled = false;
        }

        public void Show(Vector3 start, Vector3 end)
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
                ConfigureRenderer();
            }

            start.z = 0f;
            end.z = 0f;

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
