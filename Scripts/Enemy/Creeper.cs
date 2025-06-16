using System.Collections;
using FirstGearGames.SmoothCameraShaker;
using UnityEngine;

public class KamikazeeSlime : MonoBehaviour
{
    public ParticleSystem collisionParticleSystem;
    public SpriteRenderer spriteRenderer;
    public ShakeData cameraShakeData;

    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Configuration")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float explosionDelay = 2f;

    private static readonly System.Collections.Generic.Dictionary<PlayerConfig, float> pendingDamage = new();
    private bool hasExploded = false;
    private static readonly int EXPLODE_TRIGGER = Animator.StringToHash("Explode");

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasExploded && collision.CompareTag("Player"))
        {
            hasExploded = true;
            if (animator != null)
                animator.SetTrigger(EXPLODE_TRIGGER);
            StartCoroutine(ExplodeAfterDelay(collision));
        }
    }

    private IEnumerator ExplodeAfterDelay(Collider2D collision)
    {
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            Color glowColor = new Color(1f, 0.8f, 0.2f, 1f); // Warm yellow-orange glow
            float elapsed = 0f;
            
            // Increase material emission if sprite uses lit materials
            var material = spriteRenderer.material;
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
            }

            while (elapsed < explosionDelay)
            {
                // Faster pulsing (6f instead of 4f) and more intense color lerp
                float t = Mathf.PingPong(elapsed * 6f, 1f);
                // Add extra intensity to the glow color
                float intensity = 1f + Mathf.PingPong(elapsed * 8f, 2f);
                Color targetColor = glowColor * intensity;
                spriteRenderer.color = Color.Lerp(originalColor, targetColor, t);

                // Update emission if available
                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", targetColor * intensity);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Final bright flash
            spriteRenderer.color = Color.white * 2f;
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.white * 4f);
            }
        }
        else
        {
            yield return new WaitForSeconds(explosionDelay);
        }

        if (collisionParticleSystem != null)
        { 
            collisionParticleSystem.Play();
            PlayOneShot(explosionSound);
            Destroy(gameObject, 1.5f);
        }

        // Try to damage the player using the latest reference
        PlayerConfig playerConfig = collision.GetComponent<PlayerConfig>();
        if (playerConfig == null)
        {
            // Try to find the player by tag in case the collider reference is lost
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerConfig = playerObj.GetComponent<PlayerConfig>();
        }

        // Get blast radius from CircleCollider2D
        var circleCollider = GetComponent<CircleCollider2D>();
        Vector2 center = circleCollider != null ? 
            (Vector2)circleCollider.transform.position + circleCollider.offset :
            transform.position;
        float radius = circleCollider != null ? 
            circleCollider.radius * Mathf.Abs(circleCollider.transform.lossyScale.x) :
            1f;

        // Check and damage player if in range
        if (playerConfig != null)
        {
            if (circleCollider == null || Vector2.Distance(playerConfig.transform.position, center) <= radius)
            {
                lock (pendingDamage)
                {
                    if (pendingDamage.ContainsKey(playerConfig))
                        pendingDamage[playerConfig] += damage;
                    else
                        pendingDamage[playerConfig] = damage;
                }
                StartCoroutine(ApplyStackedDamage(playerConfig));
            }
        }

        // Find and damage other enemies in blast radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(center, radius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue; // Skip self

            Enemy enemyScript = hitCollider.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(100f);
            }
        }

        if (cameraShakeData != null)
            CameraShakerHandler.Shake(cameraShakeData);

        var enemy = GetComponent<Enemy>();
        if (enemy != null)
            enemy.Die();
    }

    private IEnumerator ApplyStackedDamage(PlayerConfig player)
    {
        yield return new WaitForEndOfFrame(); // Wait for other explosions in the same frame
        float totalDamage;
        lock (pendingDamage)
        {
            if (!pendingDamage.TryGetValue(player, out totalDamage))
                yield break;
            pendingDamage.Remove(player);
        }
        player.TakeDamage(totalDamage);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
