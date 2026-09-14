using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerWallJump : MonoBehaviour
{
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.08f;
    [SerializeField] private float wallCoyoteTime = 0.1f;
    [SerializeField] private float wallJumpSpeed = 8.0f;
    [SerializeField] private float wallJumpHorizontalSpeed = 7.0f;
    [SerializeField] private float movementLockTime = 0.15f;
    [SerializeField] private float wallJumpGravityMultiplier = 1.0f;
    [SerializeField] private AudioClip wallJumpSfx;

    Player player;
    Rigidbody2D playerCharacter;
    CapsuleCollider2D playerBodyCollider;
    AudioSource audioSource;
    float lastWallTime;
    float movementLockTimer;
    float startingGravityScale;
    int wallDirection;
    Vector2 wallJumpVelocity;

    void Awake()
    {
        player = GetComponent<Player>();
        playerCharacter = GetComponent<Rigidbody2D>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
        startingGravityScale = playerCharacter.gravityScale;

        if (wallLayer.value == 0)
        {
            wallLayer = player.GroundLayer;
        }
    }

    void Update()
    {
        if (!player.IsAlive)
        {
            return;
        }

        if (movementLockTimer > 0)
        {
            movementLockTimer -= Time.deltaTime;
            if (movementLockTimer <= 0)
            {
                player.SetSpecialMovementActive(false);
                playerCharacter.gravityScale = startingGravityScale;
            }
            else
            {
                playerCharacter.linearVelocity = wallJumpVelocity;
            }

            return;
        }

        int touchingWallDirection = GetWallDirection();
        if (touchingWallDirection != 0)
        {
            wallDirection = touchingWallDirection;
            lastWallTime = wallCoyoteTime;
        }
        else
        {
            lastWallTime -= Time.deltaTime;
        }

        if (player.JumpPressedThisFrame && lastWallTime > 0)
        {
            StartWallJump();
        }
    }

    private void StartWallJump()
    {
        player.SetSpecialMovementActive(true);
        movementLockTimer = movementLockTime;
        lastWallTime = 0;

        playerCharacter.gravityScale = startingGravityScale * wallJumpGravityMultiplier;
        wallJumpVelocity = new Vector2(-wallDirection * wallJumpHorizontalSpeed, wallJumpSpeed);
        playerCharacter.linearVelocity = wallJumpVelocity;

        if (audioSource != null && wallJumpSfx != null)
        {
            audioSource.PlayOneShot(wallJumpSfx);
        }
    }

    private int GetWallDirection()
    {
        Bounds bounds = playerBodyCollider.bounds;

        Vector2 leftOrigin = new Vector2(bounds.min.x, bounds.center.y);
        Vector2 rightOrigin = new Vector2(bounds.max.x, bounds.center.y);
        bool touchingLeftWall = Physics2D.Raycast(leftOrigin, Vector2.left, wallCheckDistance, wallLayer);
        bool touchingRightWall = Physics2D.Raycast(rightOrigin, Vector2.right, wallCheckDistance, wallLayer);

        if (touchingLeftWall)
        {
            return -1;
        }

        if (touchingRightWall)
        {
            return 1;
        }

        return 0;
    }
}
