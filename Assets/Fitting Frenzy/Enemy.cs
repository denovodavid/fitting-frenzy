using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 90;

    Rigidbody2D rb;

    void Awake() {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Vector2 dir = (other.transform.position - transform.position).normalized;
            rb.AddForce(dir * speed);
        }
    }
}
