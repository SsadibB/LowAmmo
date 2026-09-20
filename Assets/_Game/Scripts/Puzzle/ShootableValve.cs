using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    public class ShootableValve : MonoBehaviour, IShootable
    {
        [Header("Valve Settings")]
        [SerializeField] private Transform wheelTransform;
        [SerializeField] private float rotationAngle = 90f;
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private bool canBeToggled = false;
        [SerializeField] private AudioClip turnSound;

        [Header("Events")]
        public UnityEvent onValveOpened;
        public UnityEvent onValveClosed;

        private bool isOpen = false;
        private bool isAnimating = false;
        private Quaternion targetRotation;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            if (wheelTransform == null)
            {
                wheelTransform = transform;
            }
            targetRotation = wheelTransform.localRotation;
        }

        private void Update()
        {
            if (isAnimating)
            {
                wheelTransform.localRotation = Quaternion.RotateTowards(wheelTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
                if (Quaternion.Angle(wheelTransform.localRotation, targetRotation) < 0.1f)
                {
                    wheelTransform.localRotation = targetRotation;
                    isAnimating = false;
                }
            }
        }

        public void OnHit(RaycastHit2D hit)
        {
            if (isOpen && !canBeToggled) return;

            ToggleValve();
        }

        public void ToggleValve()
        {
            isOpen = !isOpen;

            float currentZ = wheelTransform.localEulerAngles.z;
            float targetZ = isOpen ? (currentZ + rotationAngle) : (currentZ - rotationAngle);
            targetRotation = Quaternion.Euler(0f, 0f, targetZ);
            isAnimating = true;

            if (turnSound != null)
            {
                AudioSource.PlayClipAtPoint(turnSound, transform.position);
            }

            if (isOpen)
            {
                onValveOpened?.Invoke();
            }
            else
            {
                onValveClosed?.Invoke();
            }
        }
    }
}
