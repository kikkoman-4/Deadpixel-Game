using UnityEngine;

public class CamMovement : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        Vector3 desiredPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, speed * Time.deltaTime);
    }
}
