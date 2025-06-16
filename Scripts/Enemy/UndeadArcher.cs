using UnityEngine;
using System.Collections;

public class UndeadArcher : MonoBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private GameObject projectilePrefab; // Projectile prefab to shoot at the player
    [Range(1f, 5f)]
    [SerializeField] private float shootingTime = 2.5f; // Projectile delay per next shot

    // Variables
    private Transform player;
    private Rigidbody2D rb;
    private float shootCooldown = 0f;
    private Enemy enemyComponent;
    private Animator animator;
    private const string SHOOT_TRIGGER = "Shoot";
    private bool isShooting = false;

    // Initialize Variables
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyComponent = GetComponent<Enemy>();
        animator = GetComponent<Animator>();

        if (enemyComponent == null)
        {
            Debug.LogError("Enemy component not found on UndeadArcher!");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on UndeadArcher!");
        }

        // Look for Player once spawned
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player not found.");
        }
    }

    private void Update()
    {
        if (enemyComponent == null || isShooting) return;

        // Only update shooting when player is in range
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        bool isPlayerInRange = distanceToPlayer <= enemyComponent.detectionRange;

        if (isPlayerInRange)
        {
            // Update shoot cooldown timer
            if (shootCooldown > 0f)
            {
                shootCooldown -= Time.deltaTime;
            }
            else
            {
                StartCoroutine(ShootSequence());
                shootCooldown = shootingTime; // Reset cooldown after shooting
            }
        }
    }

    public void Shoot()
    {
        if (projectilePrefab == null || player == null) return;

        // Trigger shoot animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger(SHOOT_TRIGGER);
        }

        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Calculate the angle for the projectile (convert direction to angle)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Spawn projectile at archer's position with calculated rotation
        GameObject projectile = Instantiate(projectilePrefab, transform.position, rotation);

        // Try to set velocity if the projectile has a Rigidbody2D
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            const float PROJECTILE_SPEED = 5f;
            projRb.linearVelocity = direction * PROJECTILE_SPEED;
        }
        else
        {
            Debug.LogWarning("Projectile prefab is missing Rigidbody2D component!");
            projectile.transform.right = direction;
        }
    }

    private IEnumerator ShootSequence()
    {
        isShooting = true;
        
        // Disable movement in Enemy component
        enemyComponent.canMove = false;

        // Stop any existing velocity
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Trigger shoot animation if animator exists
        if (animator != null)
        {
            animator.SetTrigger(SHOOT_TRIGGER);
        }

        // Wait for animation to reach the point where the arrow should be fired
        float timeToShoot = 0.5f; // Adjust this value based on your animation timing
        yield return new WaitForSeconds(timeToShoot);

        // Instantiate the projectile
        InstantiateProjectile();

        // Wait for remaining animation to complete
        float remainingAnimationTime = 0.5f;
        yield return new WaitForSeconds(remainingAnimationTime);

        // Re-enable movement
        enemyComponent.canMove = true;
        isShooting = false;
    }

    private void InstantiateProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Calculate the angle for the projectile
        // Subtract 90 degrees because Unity's sprites typically point up by default
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Spawn projectile at archer's position with calculated rotation
        GameObject projectile = Instantiate(projectilePrefab, transform.position, rotation);

        // Try to set velocity if the projectile has a Rigidbody2D
        Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
        if (projRb != null)
        {
            const float PROJECTILE_SPEED = 5f;
            projRb.linearVelocity = direction * PROJECTILE_SPEED;
        }
    }

    // Animation event method that can be called at the end of the shoot animation
    public void OnShootAnimationComplete()
    {
        isShooting = false;
    }
}
