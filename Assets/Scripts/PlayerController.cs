using UnityEngine;
using DG.Tweening;


public class PlayerController : MonoBehaviour
{
    [SerializeField] MapGenerator mapGenerator;
    Vector3 initialSpawnPosition;
    CircleCollider2D playerCollider;

    [Header("Movement Settings")]
    [SerializeField] float movementSpeed = 5f;
    float horizontalInput;

    [Header("Jump Settings")]
    [SerializeField] float jumpForce = 10f;
    [SerializeField] float groundCheckDistance = 0.1f;
    bool hasAlreadyJumped = false;
    bool isGrounded;

    [Header("Head Bonk Settings")]
    [SerializeField] float headCheckDistance = 0.1f;
    [SerializeField] float bonkBackForce = 5f;
    float lastBonkTime = -1f;
    float headBonkCooldown = 0.3f;

    [Header("Ground Pound Settings")]
    [SerializeField] float groundPoundForce = 20f;
    public bool IsGroundPounding => isGroundPounding;
    bool isGroundPounding;

    // References for player.
    Animator animator;
    Rigidbody2D rb2D;
    SpriteRenderer spriteRenderer;
    

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialSpawnPosition = transform.position;
        playerCollider = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        // Ground check.
        isGrounded = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.2f), 0f, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));

        if (isGrounded && isGroundPounding)
        {
            // Break a tile if we're pounding.
            GameObject groundTile = Physics2D.BoxCast(transform.position, new Vector2(0.5f, 0.2f), 0f, Vector3.down, groundCheckDistance, LayerMask.GetMask("Ground")).collider?.gameObject;
            if (groundTile != null)
            {
                BrickBreak brickBreak = groundTile.GetComponent<BrickBreak>();
                if (brickBreak != null)
                {
                    brickBreak.BreakTile();
                }
            }
            // Just one though.
            isGroundPounding = false;
        }

        // Handles movement.
        horizontalInput = Input.GetAxis("Horizontal");
        rb2D.linearVelocity = new Vector2(horizontalInput * movementSpeed, rb2D.linearVelocity.y);

        // Handles jumping.
        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && !isGroundPounding)
        {
            hasAlreadyJumped = true;
            AudioManager.Instance.PlaySound("Jump");
            rb2D.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (!isGrounded && Input.GetKeyDown(KeyCode.S) && !isGroundPounding)
        {
            // Pound that ground!
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

        // Check for tile above head, so we can bonk it.
        if (rb2D.linearVelocity.y > 0)
        {
            if (Time.time > lastBonkTime + headBonkCooldown)
            {
                HeadCheck();
            }
        }

        // If player falls below map, reset position.
        if (transform.position.y < -mapGenerator.WellHeight)
        {
            ResetPlayer();
        }
    }

    /// <summary>
    /// Check if tile is above head, to bonk it.
    /// </summary>
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

    /// <summary>
    /// Ground pound sequence. Dotween is fun.
    /// </summary>
    private void GroundPound()
    {
        isGroundPounding = true;
        AudioManager.Instance.PlaySound("GroundPound");
        // Creates a rotation animation with easing, once it finishes, slam the ground.
        Sequence rotationSequence = DOTween.Sequence();
        rb2D.constraints = RigidbodyConstraints2D.FreezePosition;
        rb2D.angularVelocity = 0f;
        rotationSequence.Append(transform.DORotate(new Vector3(0, 0, -360), 0.2f, RotateMode.FastBeyond360).SetEase(Ease.InQuad));
        rotationSequence.OnComplete(() =>
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb2D.linearVelocity = new Vector2(0, -groundPoundForce);
        });
    }

    private void ResetPlayer()
    {
        Sequence resetSequence = DOTween.Sequence();
        Camera mainCam = Camera.main;
        mainCam.transform.SetParent(transform);
        rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        playerCollider.enabled = false;
        resetSequence.Append(transform.DOMove(initialSpawnPosition + new Vector3(0, 1f, 0), 2f).SetEase(Ease.InOutQuad));
        resetSequence.Join(transform.DORotate(new Vector3(0, 0, -360), 2f, RotateMode.FastBeyond360).SetEase(Ease.Linear));
        resetSequence.OnComplete(() =>
        {
            isGroundPounding = false;
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            playerCollider.enabled = true;
            mainCam.transform.SetParent(null);
        });

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position + Vector3.down * groundCheckDistance, new Vector3(0.5f, 0.2f, 0));
    }
}