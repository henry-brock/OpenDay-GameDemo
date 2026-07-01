using UnityEngine;
using UnityEngine.InputSystem;

namespace OpenDay
{
    /// <summary>
    /// 2D platformer player movement driven by the new Input System.
    /// Horizontal run + jump, controller-first (Xbox pad), with a ground check.
    ///
    /// Setup (in the Unity Editor):
    ///   1. Add this component to your Player GameObject.
    ///   2. Add a Rigidbody2D (Gravity Scale ~3, Freeze Z rotation, Interpolate).
    ///   3. Add a Collider2D (e.g. CapsuleCollider2D).
    ///   4. Create an empty child "GroundCheck" at the player's feet and assign it
    ///      to Ground Check below; set Ground Layer to your ground/tilemap layer.
    ///   5. Add a PlayerInput component, assign the InputSystem_Actions asset,
    ///      set Default Map = "Player" and Behavior = "Send Messages".
    /// The PlayerInput component calls OnMove / OnJump below.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Run")]
        [Tooltip("Horizontal speed in units per second.")]
        [SerializeField] private float moveSpeed = 7f;

        [Header("Jump")]
        [Tooltip("Initial upward velocity applied when jumping.")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Extra gravity multiplier while falling, for a snappier arc.")]
        [SerializeField] private float fallGravityMultiplier = 2f;

        [Header("Ground check")]
        [Tooltip("Empty transform positioned at the player's feet.")]
        [SerializeField] private Transform groundCheck;

        [Tooltip("Radius of the ground-check overlap circle.")]
        [SerializeField] private float groundCheckRadius = 0.15f;

        [Tooltip("Which layers count as ground.")]
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D _rigidbody;
        private float _moveInput;      // -1..1 horizontal
        private bool _jumpQueued;      // set by input, consumed in FixedUpdate
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Ground state is read every frame so the jump feels responsive.
            _isGrounded = groundCheck != null &&
                Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        private void FixedUpdate()
        {
            // Horizontal velocity is set directly; vertical is left to physics/jump.
            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = _moveInput * moveSpeed;

            if (_jumpQueued && _isGrounded)
            {
                velocity.y = jumpForce;
            }
            _jumpQueued = false;

            _rigidbody.linearVelocity = velocity;

            // Heavier gravity on the way down gives a tighter, less floaty jump.
            _rigidbody.gravityScale = _rigidbody.linearVelocity.y < 0f ? fallGravityMultiplier : 1f;
        }

        /// <summary>Called by PlayerInput (Send Messages) for the "Move" action.</summary>
        private void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>().x;
        }

        /// <summary>Called by PlayerInput (Send Messages) for the "Jump" action.</summary>
        private void OnJump(InputValue value)
        {
            // Queue on press; FixedUpdate decides if we're actually grounded.
            if (value.isPressed)
            {
                _jumpQueued = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
