using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float runSpeed = 5.0f;
    // Acceleration makes movement feel responsive without being instant
    [SerializeField] private float runAcceleration = 30.0f;
    [SerializeField] private float runDeceleration = 40.0f;
    [SerializeField] private float jumpSpeed = 5.0f;
    // The next two variables make jumps easier to land and time
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    // Releasing jump early cuts the jump short
    [SerializeField] [Range(0.1f, 1f)] private float jumpCutMultiplier = 0.5f;
    // Falling faster keeps the controls from feeling floaty
    [SerializeField] private float fallGravityMultiplier = 2.0f;
    [SerializeField] private float climbSpeed = 5.0f;
    [SerializeField] Vector2 deathSeq = new Vector2(25f, 25f);
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private LayerMask groundLayer;

    AudioSource audioSource;
    [SerializeField] private AudioClip jumpSound;

    // A brief window after jumping off a ladder, so climbing does not cancel the jump
    [SerializeField] private float ladderJumpTime = 0.15f;
    float jumpedOffLadderTimer;

    float gravityScaleAtStart;
    float lastGroundedTime;
    float jumpBufferTimer;
    LayerMask climbingLayer;

    bool isAlive = true;

    InputAction moveAction;
    InputAction jumpAction;

    public bool IsAlive => isAlive;
    public InputActionAsset InputActions => inputActions;
    public LayerMask GroundLayer => groundLayer.value != 0 ? groundLayer : LayerMask.GetMask("Ground");
    public Vector2 MoveInput { get; private set; }
    public bool JumpPressedThisFrame => jumpAction != null && jumpAction.WasPressedThisFrame();
    // Optional movement components can report when they are controlling the Rigidbody
    public bool SpecialMovementActive { get; private set; }

    Rigidbody2D playerCharacter;

    Animator playerAnimator;

    CapsuleCollider2D playerBodyCollider;

    BoxCollider2D playerFeetCollider;

    private void Awake()
    {
        playerCharacter = GetComponent<Rigidbody2D>();
        // The Animator lives on the child sprite object
        playerAnimator = GetComponentInChildren<Animator>();
        playerBodyCollider = GetComponent<CapsuleCollider2D>();
        playerFeetCollider = GetComponent<BoxCollider2D>();

        audioSource = GetComponent<AudioSource>();

        if (inputActions == null)
        {
            Debug.LogError("Assign the Input System asset to Player.", this);
            return;
        }

        // playerMap is the group of player actions inside the Input Actions asset
        InputActionMap playerMap = inputActions.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        jumpAction = playerMap.FindAction("Jump", true);
        playerMap.Enable();

        gravityScaleAtStart = playerCharacter.gravityScale;
        climbingLayer = LayerMask.GetMask("Climbing");
        // The controller handles acceleration, so Rigidbody damping must not cancel it
        playerCharacter.linearDamping = 0.0f;

        runAcceleration = Mathf.Max(runAcceleration, runSpeed * 6.0f);
        runDeceleration = Mathf.Max(runDeceleration, runSpeed * 8.0f);
    }

    private void OnDestroy()
    {
        inputActions?.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
        {
            // Return stops the method
            return;
        }

        MoveInput = moveAction.ReadValue<Vector2>();

        Run();
        FlipSprite();
        Jump();
        Climb();
        BetterGravity();
        Die();
    }

    public void SetSpecialMovementActive(bool active)
    {
        SpecialMovementActive = active;
    }

    private void Run()
    {
        // Move toward the target speed instead of changing speed instantly
        float hMovement = MoveInput.x;
        float targetSpeed = hMovement * runSpeed;
        float speedChange = Mathf.Abs(targetSpeed) > Mathf.Epsilon ? runAcceleration : runDeceleration;
        float newSpeed = Mathf.MoveTowards(playerCharacter.linearVelocity.x, targetSpeed, speedChange * Time.deltaTime);
        playerCharacter.linearVelocity = new Vector2(newSpeed, playerCharacter.linearVelocity.y);

        bool hSpeed = Mathf.Abs(playerCharacter.linearVelocity.x) > Mathf.Epsilon;

        playerAnimator.SetBool("run", hSpeed);
    }

    private void FlipSprite()
    {
        // If the player is moving horizontally
        bool hMovement = Mathf.Abs(playerCharacter.linearVelocity.x) > Mathf.Epsilon;

        if (hMovement)
        {
            // Reverse the current direction (scale) of the X-Axis
            transform.localScale = new Vector2(Mathf.Sign(playerCharacter.linearVelocity.x), 1f);
        }
    }

    private void Jump()
    {
        // Releasing jump early produces a shorter jump
        if (jumpAction.WasReleasedThisFrame() && playerCharacter.linearVelocity.y > 0)
        {
            playerCharacter.linearVelocity = new Vector2(playerCharacter.linearVelocity.x,
                playerCharacter.linearVelocity.y * jumpCutMultiplier);
        }

        // Touching a ladder counts as ground, so the Player can jump off it
        bool isGrounded = playerFeetCollider.IsTouchingLayers(GroundLayer)
            || playerBodyCollider.IsTouchingLayers(climbingLayer);

        if (isGrounded)
        {
            // Remember a brief window after leaving a platform
            lastGroundedTime = coyoteTime;
        }
        else
        {
            lastGroundedTime -= Time.deltaTime;
        }

        if (JumpPressedThisFrame)
        {
            // Remember a jump pressed just before landing
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (lastGroundedTime <= 0 || jumpBufferTimer <= 0)
        {
            return;
        }

        playerCharacter.linearVelocity = new Vector2(playerCharacter.linearVelocity.x, jumpSpeed);
        lastGroundedTime = 0;
        jumpBufferTimer = 0;
        // Let the jump occur for a moment instead of Climb() taking over
        jumpedOffLadderTimer = ladderJumpTime;
        audioSource.PlayOneShot(jumpSound, 0.7f);
    }

    private void Climb()
    {
        // Count down the period that follows jumping off a ladder
        jumpedOffLadderTimer -= Time.deltaTime;

        if (jumpedOffLadderTimer > 0 || !playerBodyCollider.IsTouchingLayers(climbingLayer))
        {
            playerAnimator.SetBool("climb", false);

            playerCharacter.gravityScale = gravityScaleAtStart;

            // Will stop this function unless true
            return;
        }

        // Support both the Vertical axis and the standard climb keys
        float vMovement = MoveInput.y;

        // Drop horizontal momentum so the Player doesn't glide sideways on the ladder
        Vector2 climbVelocity = new Vector2(MoveInput.x * runSpeed, vMovement * climbSpeed);
        playerCharacter.linearVelocity = climbVelocity;

        playerCharacter.gravityScale = 0.0f;

        bool vSpeed = Mathf.Abs(playerCharacter.linearVelocity.y) > Mathf.Epsilon;
        playerAnimator.SetBool("climb", vSpeed);
    }

    private void BetterGravity()
    {
        if (playerBodyCollider.IsTouchingLayers(climbingLayer))
        {
            return;
        }

        // Use stronger gravity while falling, and then cap the fall speed
        float gravityMultiplier = playerCharacter.linearVelocity.y < 0 ? fallGravityMultiplier : 1f;
        playerCharacter.gravityScale = gravityScaleAtStart * gravityMultiplier;

        if (playerCharacter.linearVelocity.y < -jumpSpeed * fallGravityMultiplier)
        {
            playerCharacter.linearVelocity = new Vector2(playerCharacter.linearVelocity.x,
                -jumpSpeed * fallGravityMultiplier);
        }
    }

    private void Die()
    {
        if (playerBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazards")))
        {
            isAlive = false;
            playerAnimator.SetTrigger("die");
            playerCharacter.linearVelocity = deathSeq;

            GameSession.Instance.ProcessPlayerDeath();
        }
    }
}
