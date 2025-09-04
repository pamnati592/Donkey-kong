using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite climbSprite;    

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [Header("Layers")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask ladderMask;

    [Header("Movement")]
    [SerializeField] public float moveSpeed = 3f;
    [SerializeField] public float jumpStrength = 4f;

    [Header("Animation FPS")]
    [SerializeField] private float runFps = 12f;
    [SerializeField] private float climbFps = 8f;

    private readonly Collider2D[] overlaps = new Collider2D[8];
    private Vector2 direction;
    private bool grounded;
    private bool climbing;

    private int spriteIndex;
    private Coroutine animRoutine;
 

    private void OnEnable()
    {
        animRoutine = StartCoroutine(AnimateLoop());
    }

    private void OnDisable()
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = null;
    }

    private void Update()
    {
        CheckCollision();
        SetDirection();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * Time.fixedDeltaTime);
    }

    private static bool IsInMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;

    private void CheckCollision()
    {
        grounded = false;
        climbing = false;

        float skinWidth = 0.1f;

        Vector2 size = capsuleCollider.bounds.size;
        size.y += skinWidth;
        size.x /= 2f;

        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(groundMask | ladderMask);

        int amount = Physics2D.OverlapBox((Vector2)transform.position, size, 0f, filter, overlaps);

        for (int i = 0; i < amount; i++)
        {
            var col = overlaps[i];
            var hit = col.gameObject;

            if (IsInMask(hit.layer, groundMask))
            {
                grounded = hit.transform.position.y < (transform.position.y - 0.5f + skinWidth);
                Physics2D.IgnoreCollision(col, capsuleCollider, !grounded);
            }
            else if (IsInMask(hit.layer, ladderMask))
            {
                climbing = true;
            }
        }
    }

    private void SetDirection()
    {
        if (climbing)
        {
            direction.y = Input.GetAxis("Vertical") * moveSpeed;
        }
        else if (grounded && Input.GetButtonDown("Jump"))
        {
            direction = Vector2.up * jumpStrength;
        }
        else
        {
            direction += Physics2D.gravity * Time.deltaTime;
        }

        direction.x = Input.GetAxis("Horizontal") * moveSpeed;

        if (grounded)
        {
            direction.y = Mathf.Max(direction.y, -1f);
        }

        if (direction.x > 0f)
            transform.eulerAngles = Vector3.zero;
        else if (direction.x < 0f)
            transform.eulerAngles = new Vector3(0f, 180f, 0f);
    }

    private IEnumerator AnimateLoop()
    {
        while (true)
        {
            bool movingHorizontally = Mathf.Abs(direction.x) > 0.01f;

            float fps = climbing ? climbFps : (movingHorizontally ? runFps : runFps);
            float delay = (fps <= 0f) ? (1f / 12f) : (1f / fps);

            if (climbing)
            {
               
              spriteRenderer.sprite = climbSprite;
                
            }
            else if (movingHorizontally && runSprites != null && runSprites.Length > 0)
            {
                spriteIndex = (spriteIndex + 1) % runSprites.Length;
                spriteRenderer.sprite = runSprites[spriteIndex];
            }
            else
            {

                spriteRenderer.sprite = runSprites[0];
            }

            yield return new WaitForSeconds(delay);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Objective"))
        {
            enabled = false;
            GameManager.Instance.LevelComplete();
        }
        else if (collision.gameObject.CompareTag("Obstacle"))
        {
            enabled = false;
            GameManager.Instance.LevelFailed();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!capsuleCollider) return;
        float skinWidth = 0.1f;
        Vector2 size = capsuleCollider.bounds.size;
        size.y += skinWidth;
        size.x /= 2f;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
