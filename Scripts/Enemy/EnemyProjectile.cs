using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    // Configuration
    [Header("Projectile Settings")]
    public float projectileSpeed = 10f;
    public float knockbackForce = 5f; // Force applied to the player on hit

    // Variables
    private Rigidbody2D rb;
    private GameObject playerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerObject = collision.gameObject;
            UndeadArcher undeadArcher = GetComponentInParent<UndeadArcher>();
            playerObject.GetComponent<PlayerConfig>().TakeDamage(10f);
            Destroy(gameObject);

            // Calculate knockback direction and value
            Vector2 knockbackDirection = (playerObject.transform.position - transform.position).normalized;
            Rigidbody2D playerRb = playerObject.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
            }
        }
        else if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            //Debug.Log("Projectile hit the wall!");
        }
    }
}
