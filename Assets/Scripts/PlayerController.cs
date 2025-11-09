using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundCheckDistance = 0.1f;

    Animator animator;
    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
    bool isGrounded;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Ground check.
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));

        // Movement.
        float horizontalInput = Input.GetAxis("Horizontal");
        rb2D.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb2D.linearVelocity.y);

        // Jumping.
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (!isGrounded && Input.GetKeyDown(KeyCode.S))
        {
            GroundPound();
        }

        // Basic animations.
        animator.SetBool("isMoving", horizontalInput != 0);
        animator.SetBool("isGrounded", isGrounded);

        // Flip sprite based on input.
        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
    }

    private void GroundPound()
    {
        // TODO: Implement ground pound logic.
    }
}