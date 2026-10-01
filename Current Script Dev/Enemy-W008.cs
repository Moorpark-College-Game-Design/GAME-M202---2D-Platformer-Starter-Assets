using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.0f;

    Rigidbody2D enemyCharacter;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyCharacter = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(IsFacingRight())
        {
            enemyCharacter.linearVelocity = new Vector2(moveSpeed, 0);
        }
        else
        {
            enemyCharacter.linearVelocity = new Vector2(-moveSpeed, 0);
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        transform.localScale = new Vector2(-(Mathf.Sign(enemyCharacter.linearVelocity.x)), 1.0f);
    }

    bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }

}
