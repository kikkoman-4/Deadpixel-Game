using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    [SerializeField] public float detectionRange = 10f;
    [SerializeField] private float stoppingDistance = 0.3f;
    [SerializeField] private float randomMovementInterval = 2f;

    private Transform player;
    public Rigidbody2D rb;
    private Vector2 movement;
    private bool isPlayerInRange;
    private float nextDirectionChange;
    public bool canMove = true;

    [Range(0f, 25f)][SerializeField] private float damage = 5f;
    [SerializeField] private float damageInterval = 0.5f;
    private float lastDamageTime;

    [Range(0f, 25f)][SerializeField] private float playerExitTime;
    [SerializeField] private float maxHealth = 10f; // Default max health 10
    private float currentHealth;
    private GameObject playerObject;
    public bool hasEnteredPve;

    // Enemy HP UI
    [SerializeField] FloatingHealthBar healthBar;

    // Color flash
    [SerializeField] private float hitFlashDuration = 0.5f;
    private SpriteRenderer spriteRenderer;
    private Coroutine flashCoroutine;

    [SerializeField] private float stunDuration = 1f;
    private bool isStunned = false;

    private void Awake()
    {
        currentHealth = maxHealth;

        healthBar = GetComponentInChildren<FloatingHealthBar>();
        healthBar.GetComponentInChildren<Slider>().gameObject.SetActive(false);

        spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Randomize movement speed if this is a zombie
        if (gameObject.name.ToLower().Contains("zombie"))
        {
            // Random speed between 3 and 7
            moveSpeed = Random.Range(2f, 4f);
        }

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
        if (player == null || isStunned) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= detectionRange;

        // Handle movement behavior
        if (isPlayerInRange)
        {
            // Follow player
            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
            
            // Only move if we're further than stopping distance
            if (distanceToPlayer > stoppingDistance)
            {
                movement = direction;
            }
            else
            {
                movement = Vector2.zero;
            }
        }
        else
        {
            // Random movement when player is out of range
            if (Time.time >= nextDirectionChange)
            {
                // Generate random direction
                float randomAngle = Random.Range(0f, 360f);
                movement = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                
                // Set next direction change time
                nextDirectionChange = Time.time + randomMovementInterval;
            }
        }
    }

    private void FixedUpdate()
    {
        if (!canMove || isStunned) return;

        if (rb != null)
        {
            Vector2 targetVelocity = movement * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
            
            // Prevent sticking to walls
            if (rb.linearVelocity.magnitude < 0.1f && movement.magnitude > 0.1f)
            {
                rb.linearVelocity = movement * moveSpeed * 0.5f;
            }

            // Flip sprite based on movement direction
            if (rb.linearVelocity.x != 0 && spriteRenderer != null)
            {
                // Flip if moving left, unflip if moving right
                spriteRenderer.flipX = rb.linearVelocity.x < 0;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerProjectile"))
        {
            // Handle bullet collision
            TakeDamage(5f); // Example bullet damage, adjust as needed
            Destroy(collision.gameObject);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Early return if not a zombie
        if (GetComponent<Zombie>() == null) return;

        if (collision.gameObject.CompareTag("Player") && !isStunned)
        {
            if (Time.time >= lastDamageTime + damageInterval)
            {
                var player = collision.gameObject.GetComponent<PlayerConfig>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    lastDamageTime = Time.time;
                    
                    // Play attack animation
                    var animator = GetComponent<Animator>();
                    if (animator != null)
                    {
                        //animator.Play("attack");
                    }
                    
                    // Start stun coroutine
                    StartCoroutine(StunAfterAttack());
                }
            }
        }
    }

    private IEnumerator StunAfterAttack()
    {
        isStunned = true;
        movement = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(stunDuration);
        
        isStunned = false;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBar.UpdateHealthBar(currentHealth, maxHealth);
        healthBar.GetComponentInChildren<Slider>().gameObject.SetActive(true);

        // Flash red on hit
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRed());
        }

        if (currentHealth <= 0f)
        {
            Die();
            healthBar.GetComponentInChildren<Slider>().gameObject.SetActive(false);
        }
    }

    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        Color hitColor = Color.red;
        float elapsed = 0f;

        // Fade to red
        while (elapsed < hitFlashDuration / 2f)
        {
            float t = elapsed / (hitFlashDuration / 2f);
            spriteRenderer.color = Color.Lerp(originalColor, hitColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = hitColor;

        // Fade back to original
        elapsed = 0f;
        while (elapsed < hitFlashDuration / 2f)
        {
            float t = elapsed / (hitFlashDuration / 2f);
            spriteRenderer.color = Color.Lerp(hitColor, originalColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = originalColor;
    }

    public void Die()
    {

        // Freeze the enemy by disabling its Rigidbody2D physics and movement
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll; // Freeze position and rotation
        }

        // Optionally disable any movement or rotation scripts here
        var movement = GetComponent<MonoBehaviour>(); // Replace with your movement/rotation script type if any
        if (movement != null)
            movement.enabled = false;

        // Optionally disable collider to prevent further interactions
        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        // Check and play death animations for different enemy types
        var animator = GetComponent<Animator>();
        
        // Check Zombie
        var zombie = GetComponent<Zombie>();
        if (zombie != null && animator != null)
        {
            animator.Play("death");
            // Wait for animation before destroying
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
            return;
        }

        // Check UndeadArcher  
        var undeadArcher = GetComponent<UndeadArcher>();
        if (undeadArcher != null && animator != null)
        {
            // Disable the enemy's (IF SKELETON) components to stop its behavior
            undeadArcher.enabled = false;
            animator.Play("death");
            // Already disabled the UndeadArcher component above
            Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
            return;
        }

        // Check KamikazeeSlime
        //var kamikazeeSlime = GetComponent<KamikazeeSlime>();
        //if (kamikazeeSlime != null && animator != null)
        //{
        //    animator.Play("death");
        //    // Add slime death effects if needed
        //    Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
        //    return;
        //}
    }
}
