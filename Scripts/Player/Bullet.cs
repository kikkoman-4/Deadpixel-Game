using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Range(1, 10)][SerializeField] private float speed = 10f;
    [Range(1, 10)][SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = transform.right * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            Destroy(gameObject);
        else if (collision.gameObject.CompareTag("Wall"))
            Destroy(gameObject);
    }
}
