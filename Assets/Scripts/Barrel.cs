using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Barrel : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 8f;

    private int dirSign = 1;


    private void Awake()
    {
        
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        var v = rb.linearVelocity;
        v.x = dirSign * speed;
        rb.linearVelocity = v;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HammerHitbox"))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Wall"))
        {
            dirSign = -dirSign;
            var v = rb.linearVelocity;
            v.x = dirSign * speed;
            rb.linearVelocity = v;
        }
        else if (col.gameObject.CompareTag("FirstFloor"))
        {
          
            Destroy(gameObject);
            
        }
    }
}
