using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostSimple : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 1.8f;
    [SerializeField] private float checkAhead = 1;   
    [SerializeField] private float checkDown = 0.6f;    

    private int dir = -1; 

    private void Awake()
    {
        
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(dir * speed, 0f);

        Vector2 origin = (Vector2)transform.position + Vector2.right * dir * checkAhead;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, checkDown);

        if (hit.collider == null || !hit.collider.CompareTag("Ground"))
            Flip();
    }

    private void Flip()
    {
        dir = -dir;
        var e = transform.eulerAngles;
        e.y = (dir < 0) ? 0f : 180f;
        transform.eulerAngles = e;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            GameManager.Instance.LevelFailed();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hammer"))
            Destroy(gameObject);
    }
}
