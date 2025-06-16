using UnityEngine;

public class Shooting : MonoBehaviour
{
    // Bullet Mechanics
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    private Camera mainCam;
    private Vector3 mousePos;

    private void Start()
    {
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Hand movements
        mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 rotation = mousePos - transform.position;
        float rotZ = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rotZ);

        // Flip sprite vertically if mouse is on the left side of the screen
        if (mousePos.x < transform.position.x)
        {
            transform.localScale = new Vector3(transform.localScale.x, -Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, Mathf.Abs(transform.localScale.y), transform.localScale.z);
        }

        // Shooting mechanics
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Add shooting animation here if needed
        // Animator animator = GetComponent<Animator>();

        GameObject bullet = Instantiate(bulletPrefab, firingPoint.position, firingPoint.rotation);

        // Add shooting sound here if needed
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}
