using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool stayPressed = false;
        [SerializeField] private Transform plateTop;
        [SerializeField] private float pressDepth = 0.15f;
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private AudioClip clickSound;

        [Header("Events")]
        public UnityEvent onPressed;
        public UnityEvent onReleased;

        private Vector3 unpressedPos;
        private Vector3 pressedPos;
        private HashSet<Collider2D> currentColliders = new HashSet<Collider2D>();
        private bool isPressed = false;

        public bool IsPressed => isPressed;

        private void Awake()
        {
            if (plateTop != null)
            {
                unpressedPos = plateTop.localPosition;
                pressedPos = unpressedPos + Vector3.down * pressDepth;
            }
        }

        private void Update()
        {
            if (plateTop != null)
            {
                Vector3 target = isPressed ? pressedPos : unpressedPos;
                plateTop.localPosition = Vector3.Lerp(plateTop.localPosition, target, smoothSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore bullets/triggers
            if (other.isTrigger) return;

            currentColliders.Add(other);
            if (!isPressed && currentColliders.Count > 0)
            {
                SetPressed(true);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.isTrigger) return;

            currentColliders.Remove(other);
            if (isPressed && !stayPressed && currentColliders.Count == 0)
            {
                SetPressed(false);
            }
        }

        private void SetPressed(bool pressed)
        {
            isPressed = pressed;
            if (isPressed)
            {
                if (clickSound != null)
                {
                    AudioSource.PlayClipAtPoint(clickSound, transform.position);
                }
                onPressed?.Invoke();
            }
            else
            {
                onReleased?.Invoke();
            }
        }
    }
}
