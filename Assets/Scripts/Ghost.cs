using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GhostSimple : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed = 6;
    [SerializeField] private float checkDown;
    [SerializeField] private float edgeOffset;
    [SerializeField] private LayerMask groundMask;

    private int dir = -1;
    private Collider2D col;

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        
      
        rb.linearVelocity = new Vector2(dir * speed*GameManager.Instance.EnemySpeedMul, 0f);

        Bounds b = col.bounds;
        float frontX = (dir > 0) ? b.max.x : b.min.x;
        Vector2 foot = new Vector2(frontX + dir * edgeOffset, b.min.y);
        Vector2 origin = foot + Vector2.up * 0.05f;

        RaycastHit2D downHit = Physics2D.Raycast(origin, Vector2.down, checkDown, groundMask);
        if (!downHit) Flip();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("HammerHitbox"))
        {
            Destroy(gameObject);
        }
    }
    private void Flip()
    {
        dir = -dir;
        var e = transform.eulerAngles;
        e.y = (dir < 0) ? 0f : 180f;
        transform.eulerAngles = e;
    }


    


}
