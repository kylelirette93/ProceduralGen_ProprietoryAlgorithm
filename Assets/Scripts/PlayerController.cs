using UnityEngine;
using DG.Tweening;


public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 5f;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundCheckDistance = 0.1f;
    [SerializeField] float headCheckDistance = 0.1f;
    [SerializeField] float bonkBackForce = 5f;

    Animator animator;
    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
    public bool isGrounded;
    public bool IsGroundPounding => isGroundPounding;   
    bool isGroundPounding;
    float lastBonkTime = -1f;
    float headBonkCooldown = 0.1f;

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

        if (isGrounded && isGroundPounding)
        {
            // Break a tile.
            GameObject groundTile = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground")).collider?.gameObject;
            if (groundTile != null)
            {
                BrickBreak brickBreak = groundTile.GetComponent<BrickBreak>();
                if (brickBreak != null)
                {
                    brickBreak.BreakTile();
                }
            }
            isGroundPounding = false;
        }

        // Movement.
        float horizontalInput = Input.GetAxis("Horizontal");
        rb2D.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb2D.linearVelocity.y);

        // Jumping.
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isGroundPounding)
        {
            AudioManager.Instance.PlaySound("Jump");
            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (!isGrounded && Input.GetKeyDown(KeyCode.S) && !isGroundPounding)
        {
            GroundPound();
        }

        // Basic animations.
        animator.SetBool("isMoving", horizontalInput != 0);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isGroundPounding", isGroundPounding);

        // Flip sprite based on input.
        if (horizontalInput != 0)
        {
            spriteRenderer.flipX = horizontalInput < 0;
        }
        // Check for tile above head.
        if (rb2D.linearVelocity.y > 0 && Time.time > lastBonkTime)
        {
            HeadCheck();
        }
    }

    private void HeadCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, headCheckDistance, LayerMask.GetMask("Ground"));

        if (hit.collider != null)
        {
            BrickBreak brickBreak = hit.collider.GetComponent<BrickBreak>();
            if (brickBreak != null)
            {
                brickBreak.BreakTile();

                rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, -bonkBackForce);

                lastBonkTime = Time.time;
            }
        }
    }

    private void GroundPound()
    {
        isGroundPounding = true;
        AudioManager.Instance.PlaySound("GroundPound");
        // TODO: Implement ground pound logic.
        Sequence rotationSequence = DOTween.Sequence();
        rb2D.constraints = RigidbodyConstraints2D.FreezePosition;
        rb2D.angularVelocity = 0f;
        rotationSequence.Append(transform.DORotate(new Vector3(0, 0, -360), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.InQuad));
        rotationSequence.OnComplete(() =>
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb2D.linearVelocity = new Vector2(0, -50f);
        });
    }
}