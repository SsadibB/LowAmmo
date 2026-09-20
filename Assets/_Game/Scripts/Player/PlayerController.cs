using UnityEngine;
using UnityEngine.InputSystem;

namespace LowAmmo.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float acceleration = 60f;
        [SerializeField] private float deceleration = 50f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 13.5f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2.0f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.15f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.15f);
        [SerializeField] private LayerMask groundLayer;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer bodySpriteRenderer;

        private Rigidbody2D rb;
        private Collider2D col;

        private float horizontalInput;
        private bool jumpRequested;
        private bool jumpHeld;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool isGrounded;
        private bool isDead;

        public bool IsGrounded => isGrounded;
        public bool IsDead => isDead;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            if (bodySpriteRenderer == null)
            {
                bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (isDead) return;

            ReadInput();

            // Ground detection
            Vector2 checkPos = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position + Vector2.down * 0.5f;
            isGrounded = Physics2D.OverlapBox(checkPos, groundCheckSize, 0f, groundLayer);

            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            if (jumpRequested)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpRequested = false;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Jump execution
            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteTimeCounter = 0f;
            }
        }

        private void FixedUpdate()
        {
            if (isDead)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            ApplyHorizontalMovement();
            ApplyBetterJumpPhysics();
        }

        private void ReadInput()
        {
            float horiz = 0f;
            bool jumpDown = false;
            bool jumpPressing = false;

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horiz -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horiz += 1f;

                if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
                    jumpDown = true;

                if (kb.spaceKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed)
                    jumpPressing = true;
            }

            horizontalInput = horiz;
            if (jumpDown) jumpRequested = true;
            jumpHeld = jumpPressing;
        }

        private void ApplyHorizontalMovement()
        {
            float targetVelocityX = horizontalInput * moveSpeed;
            float speedDiff = targetVelocityX - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetVelocityX) > 0.01f) ? acceleration : deceleration;
            float movement = speedDiff * accelRate * Time.fixedDeltaTime;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);

            // Sprite facing direction (aiming will independently handle gun direction)
            if (bodySpriteRenderer != null && Mathf.Abs(horizontalInput) > 0.05f)
            {
                bodySpriteRenderer.flipX = horizontalInput < 0f;
            }
        }

        private void ExecuteJump()
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        private void ApplyBetterJumpPhysics()
        {
            if (rb.linearVelocity.y < 0)
            {
                // Falling fast
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
            else if (rb.linearVelocity.y > 0 && !jumpHeld)
            {
                // Variable jump height when releasing button early
                rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            if (bodySpriteRenderer != null)
            {
                bodySpriteRenderer.color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Vector2 checkPos = groundCheckPoint != null ? (Vector2)groundCheckPoint.position : (Vector2)transform.position + Vector2.down * 0.5f;
            Gizmos.DrawWireCube(checkPos, groundCheckSize);
        }
    }
}
