using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(Player), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerDoubleJump : MonoBehaviour
{
    [SerializeField] private float doubleJumpSpeed = 5.0f;
    [Min(1)] [SerializeField] private int numberOfJumps = 2;

    Player player;
    Rigidbody2D playerCharacter;
    BoxCollider2D playerFeetCollider;
    int jumpsRemaining;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerCharacter = GetComponent<Rigidbody2D>();
        playerFeetCollider = GetComponent<BoxCollider2D>();

        numberOfJumps = Mathf.Max(1, numberOfJumps);
        jumpsRemaining = numberOfJumps;
    }

    private void Update()
    {
        if (!player.IsAlive)
        {
            return;
        }

        if (playerFeetCollider.IsTouchingLayers(player.GroundLayer))
        {
            jumpsRemaining = numberOfJumps;
            return;
        }

        if (player.JumpPressedThisFrame && jumpsRemaining > 1)
        {
            playerCharacter.linearVelocity = new Vector2(playerCharacter.linearVelocity.x, doubleJumpSpeed);
            jumpsRemaining--;
        }
    }

    private void OnValidate()
    {
        numberOfJumps = Mathf.Max(1, numberOfJumps);
    }
}
