using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    public class ShootableSwitch : MonoBehaviour, IShootable
    {
        [Header("Settings")]
        [SerializeField] private bool isToggle = true;
        [SerializeField] private Transform switchLever;
        [SerializeField] private float onAngle = 45f;
        [SerializeField] private float offAngle = -45f;
        [SerializeField] private SpriteRenderer statusLight;
        [SerializeField] private Color onColor = Color.green;
        [SerializeField] private Color offColor = Color.red;
        [SerializeField] private AudioClip switchSound;

        [Header("Events")]
        public UnityEvent onSwitchedOn;
        public UnityEvent onSwitchedOff;

        private bool isOn = false;
        public bool IsOn => isOn;

        private void Start()
        {
            UpdateVisuals();
        }

        public void OnHit(RaycastHit2D hit)
        {
            if (!isToggle && isOn) return;

            isOn = !isOn;
            UpdateVisuals();

            if (switchSound != null)
            {
                AudioSource.PlayClipAtPoint(switchSound, transform.position);
            }

            if (isOn)
            {
                onSwitchedOn?.Invoke();
            }
            else
            {
                onSwitchedOff?.Invoke();
            }
        }

        private void UpdateVisuals()
        {
            if (switchLever != null)
            {
                switchLever.localRotation = Quaternion.Euler(0f, 0f, isOn ? onAngle : offAngle);
            }

            if (statusLight != null)
            {
                statusLight.color = isOn ? onColor : offColor;
            }
        }
    }
}
