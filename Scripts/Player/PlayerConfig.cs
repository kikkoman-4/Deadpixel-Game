using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;
using System.Collections;

public class PlayerConfig : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject playerUI;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject playerHand;
    [SerializeField] private GameObject DeathScreen;

    [Header("Stats")]
    [SerializeField] private float healthAmount = 100f;
    [SerializeField] public float moveSpeed = 5f;
    
    [Header("Combat")]
    public ShakeData cameraShakeData;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hurtSound;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    private Vector2 movement;
    
    private bool isDead;
    public bool inPve;
    
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int HurtTrigger = Animator.StringToHash("hurt");
    private static readonly int IsDead = Animator.StringToHash("isDead");
    private const float MAX_HEALTH = 100f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start() => playerUI.SetActive(true);

    private void Update()
    {
        if (isDead) return;

        HandleMovementInput();
        CheckHealth();
        HandleCombatState();
    }

    private void HandleMovementInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        if (movement.x != 0) spriteRenderer.flipX = movement.x < 0;
        
        bool isMoving = movement.magnitude > 0;
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(IsWalking, isMoving);
        }
    }

    private void CheckHealth()
    {
        if (healthAmount <= 0)
            Die();
    }

    private void HandleCombatState()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            TakeDamage(20);
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Disable hand and trigger hurt animation
        playerHand.SetActive(false);
        if (animator != null && animator.isActiveAndEnabled)
        {
            // Reset any existing hurt trigger first
            animator.ResetTrigger(HurtTrigger);
            animator.SetTrigger(HurtTrigger);
        }

        if (audioSource != null && hurtSound != null)
        {
            audioSource.PlayOneShot(hurtSound);
        }

        healthAmount = Mathf.Max(0, healthAmount - damage);
        UpdateHealthBar();
        CameraShakerHandler.Shake(cameraShakeData);
    }

    // Called via Animation Event at the end of hurt animation
    public void OnHurtAnimationComplete()
    {
        if (!isDead) // Only re-enable hand if player is still alive
            playerHand.SetActive(true);
    }

    private void UpdateHealthBar() => healthBar.fillAmount = healthAmount / MAX_HEALTH;

    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        moveSpeed = 0f;
        rb.linearVelocity = Vector2.zero;
        
        GetComponent<Collider2D>().enabled = false;
        playerHand.SetActive(false);
        playerUI.SetActive(false);

        if (animator != null && animator.isActiveAndEnabled)
        {
            // Reset all animation states
            animator.SetBool(IsWalking, false);
            animator.ResetTrigger(HurtTrigger);
            
            // Set death parameter and use CrossFade for smooth transition
            animator.SetBool(IsDead, true);
            animator.CrossFade("death", 0.1f, 0, 0f);
        }

        // Don't disable the script until death animation finishes
        StartCoroutine(HandleDeathSequence());
    }

    private IEnumerator HandleDeathSequence()
    {
        // Wait for death animation to complete
        if (animator != null && animator.isActiveAndEnabled)
        {
            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        }

        enabled = false;
        yield return new WaitForSeconds(3f);
        DeathScreen.SetActive(true);
    }
}


