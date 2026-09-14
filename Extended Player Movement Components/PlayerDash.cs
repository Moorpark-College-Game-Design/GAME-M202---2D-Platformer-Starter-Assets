using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 12.0f;
    [SerializeField] private float dashTime = 0.15f;
    [SerializeField] private float dashCooldown = 0.1f;
    [SerializeField] private bool allowAirDash = true;
    [SerializeField] private AudioClip dashSfx;

    Player player;
    Rigidbody2D playerCharacter;
    BoxCollider2D playerFeetCollider;
    AudioSource audioSource;
    InputAction dashAction;
    float dashTimer;
    float cooldownTimer;
    float startingGravityScale;
    bool dashAvailable = true;
    Vector2 dashVelocity;

    void Awake()
    {
        player = GetComponent<Player>();
        playerCharacter = GetComponent<Rigidbody2D>();
        playerFeetCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        startingGravityScale = playerCharacter.gravityScale;
        dashAction = player.InputActions?.FindAction("Player/Dash");

    }

    void Update()
    {
        if (!player.IsAlive)
        {
            return;
        }

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (IsGrounded())
        {
            dashAvailable = true;
        }

        if (dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                EndDash();
            }
            else
            {
                playerCharacter.linearVelocity = dashVelocity;
            }

            return;
        }

        bool canDashInCurrentState = allowAirDash || IsGrounded();

        if (cooldownTimer <= 0 && dashAvailable && canDashInCurrentState && dashAction != null && dashAction.WasPressedThisFrame())
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        Vector2 dashDirection = player.MoveInput;
        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
        }

        player.SetSpecialMovementActive(true);
        playerCharacter.gravityScale = 0;
        dashVelocity = dashDirection.normalized * dashSpeed;
        playerCharacter.linearVelocity = dashVelocity;
        dashTimer = dashTime;
        cooldownTimer = dashCooldown;
        dashAvailable = false;

        if (audioSource != null && dashSfx != null)
        {
            audioSource.PlayOneShot(dashSfx);
        }
    }

    private void EndDash()
    {
        player.SetSpecialMovementActive(false);
        playerCharacter.gravityScale = startingGravityScale;
    }

    private bool IsGrounded()
    {
        return playerFeetCollider.IsTouchingLayers(player.GroundLayer);
    }
}
