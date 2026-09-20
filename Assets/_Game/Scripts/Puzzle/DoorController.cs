using UnityEngine;
using UnityEngine.Events;

namespace LowAmmo.Puzzle
{
    public class DoorController : MonoBehaviour
    {
        [Header("Door Movement")]
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private bool startsOpen = false;

        [Header("Audio & Events")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        public UnityEvent onOpened;
        public UnityEvent onClosed;

        private Vector3 closedPos;
        private Vector3 openPos;
        private bool isOpen;
        private Collider2D col;

        public bool IsOpen => isOpen;

        private void Awake()
        {
            closedPos = transform.position;
            openPos = closedPos + openOffset;
            col = GetComponent<Collider2D>();
            isOpen = startsOpen;

            if (isOpen)
            {
                transform.position = openPos;
            }
        }

        private void Update()
        {
            Vector3 target = isOpen ? openPos : closedPos;
            if (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            }
        }

        public void OpenDoor()
        {
            if (isOpen) return;
            isOpen = true;
            if (openSound != null) AudioSource.PlayClipAtPoint(openSound, transform.position);
            onOpened?.Invoke();
        }

        public void CloseDoor()
        {
            if (!isOpen) return;
            isOpen = false;
            if (closeSound != null) AudioSource.PlayClipAtPoint(closeSound, transform.position);
            onClosed?.Invoke();
        }

        public void ToggleDoor()
        {
            if (isOpen) CloseDoor();
            else OpenDoor();
        }
    }
}
