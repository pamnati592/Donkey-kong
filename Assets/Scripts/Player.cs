using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite[] runHammerSprites;
    [SerializeField] private Sprite[] attackHammerSprites;
    [SerializeField] private Sprite[] climbSprites;
    [SerializeField] private Sprite deadSprite;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float jumpStrength = 4f;

    [Header("Animation FPS")]
    [SerializeField] private float runFps = 12f;
    [SerializeField] private float climbFps = 8f;
    [SerializeField] private float attackFps = 14f;

    [Header("Hammer Attack")]
    [SerializeField] private GameObject hammerHitbox;
    [SerializeField] private float attackDuration = 0.10f;
    [SerializeField] private float attackCooldown = 0.1f;

    [Header("Ladder Exit")]
    [SerializeField] private float ladderExitBuffer = 0.2f;
    [SerializeField] private float ladderCheckUpExtra = 0.3f;

    [Header("Ladder Enter Down")]
    [SerializeField] private float enterDownOffset = 0.2f;
    [SerializeField] public AudioSource jumpAudio;
    [SerializeField] public AudioSource GetHammerOrAttack;
    [SerializeField] public AudioSource DieAudio;



    private Vector2 direction;
    private bool grounded;
    private bool climbing;

    private int spriteIndex;
    private int climbIndex;
    private Coroutine animRoutine;

    private float vInput;

    private Collider2D currentLadder;
    private float ladderTopY;
    private bool exitingLadder;

    public bool HasHammer { get; private set; }
    private bool attacking;
    private bool canAttack = true;

    private void OnEnable()
    {
        animRoutine = StartCoroutine(AnimateLoop());
        if (hammerHitbox) hammerHitbox.SetActive(false);
        
    }

    private void OnDisable()
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = null;
    }

    private void Update()
    {
        vInput = Input.GetAxisRaw("Vertical");
        CheckCollision();

        if (climbing && grounded && vInput <= 0.1f) StopClimb();

        SetDirection();

        CheckAndHandleLadderTopExit();

        if (HasHammer && !attacking && canAttack && Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(HammerAttack());
    }

    private void CheckAndHandleLadderTopExit()
    {
        if (climbing && currentLadder && !exitingLadder && vInput >= 0f)
        {
            float halfH = capsuleCollider.bounds.extents.y;
            float dist = halfH + ladderCheckUpExtra;
            var hit = Physics2D.Raycast(capsuleCollider.bounds.center, Vector2.up, dist);
            if (hit.collider != null && hit.collider.CompareTag("Ground"))
            {
                float targetY = hit.point.y + halfH + ladderExitBuffer;
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                StartCoroutine(ExitLadderSmooth());
            }
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + direction * Time.fixedDeltaTime);
    }

    private void CheckCollision()
    {
        grounded = false;

        float skinWidth = 0.1f;
        var bounds = capsuleCollider.bounds;

        Vector2 size = bounds.size;
        size.y += skinWidth;
        size.x /= 2f;

        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            if (!col) continue;
            if (col.CompareTag("Ground"))
                grounded = col.transform.position.y < (transform.position.y - 0.5f + skinWidth);
        }
    }

    private void SetDirection()
    {
        if (climbing)
        {
            SetClimbing();
        }
        else if (grounded && Input.GetButtonDown("Jump"))
        {
            SetJump();
        }
        else
        {
            SetWalking();
        }

        if (grounded) direction.y = Mathf.Max(direction.y, -1f);
        SetPlayerAngular();
    }

    private void SetPlayerAngular()
    {
        if (direction.x > 0f) transform.eulerAngles = Vector3.zero;
        else if (direction.x < 0f) transform.eulerAngles = new Vector3(0f, 180f, 0f);
    }

    private void SetWalking()
    {
        rb.gravityScale = 3f;
        direction += Physics2D.gravity * Time.deltaTime;
        direction.x = Input.GetAxis("Horizontal") * moveSpeed;
    }

    private void SetJump()
    {
        direction = Vector2.up * jumpStrength;
        rb.gravityScale = 3f;
        direction.x = Input.GetAxis("Horizontal") * moveSpeed;
        if (jumpAudio) jumpAudio.Play();
    }

    private void SetClimbing()
    {
        rb.gravityScale = 0f;
        direction.y = vInput * moveSpeed;
        direction.x = 0f;
    }

    private IEnumerator AnimateLoop()
    {
        while (true)
        {
            bool movingHorizontally = Mathf.Abs(direction.x) > 0.01f;

            if (attacking && attackHammerSprites != null && attackHammerSprites.Length > 0)
            {
                spriteIndex = (spriteIndex + 1) % attackHammerSprites.Length;
                spriteRenderer.sprite = attackHammerSprites[spriteIndex];
                yield return new WaitForSeconds(1f / Mathf.Max(attackFps, 1f));
                continue;
            }

            if (climbing)
            {
                float delay;
                if (Mathf.Abs(vInput) > 0.01f && climbSprites != null && climbSprites.Length > 0)
                {
                    climbIndex = (climbIndex + 1) % climbSprites.Length;
                    spriteRenderer.sprite = climbSprites[climbIndex];
                    delay = 1f / Mathf.Max(climbFps, 1f);
                }
                else
                {
                    if (climbSprites != null && climbSprites.Length > 0)
                        spriteRenderer.sprite = climbSprites[0];
                    delay = 1f / 12f;
                }
                yield return new WaitForSeconds(delay);
                continue;
            }

            float fps = runFps;
            float runDelay = (fps <= 0f) ? (1f / 12f) : (1f / fps);

            if (movingHorizontally)
            {
                var arr = HasHammer ? runHammerSprites : runSprites;
                if (arr != null && arr.Length > 0)
                {
                    spriteIndex = (spriteIndex + 1) % arr.Length;
                    spriteRenderer.sprite = arr[spriteIndex];
                }
            }
            else
            {
                var arr = HasHammer ? runHammerSprites : runSprites;
                if (arr != null && arr.Length > 0)
                    spriteRenderer.sprite = arr[0];
            }

            yield return new WaitForSeconds(runDelay);
        }
    }

    private IEnumerator HammerAttack()
    {
        attacking = true;
        canAttack = false;
        if (hammerHitbox) hammerHitbox.SetActive(true);
        hammerHitbox.transform.position = new Vector2(transform.position.x + (transform.eulerAngles.y == 0f ? 0.5f : -0.5f), transform.position.y);
        if (GetHammerOrAttack) GetHammerOrAttack.Play();
        yield return new WaitForSeconds(attackDuration);
        if (hammerHitbox) hammerHitbox.SetActive(false);
        attacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
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
           StartCoroutine(DeathSequence());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Objective"))
        {
            enabled = false;
            GameManager.Instance.LevelComplete();
        }
        else if (other.CompareTag("Obstacle") && !attacking)
        {
            StartCoroutine(DeathSequence());
        }
        
        else if (other.CompareTag("Hammer"))
        {
            HasHammer = true;
            spriteIndex = 0;
            Destroy(other.gameObject);
            if (GetHammerOrAttack) GetHammerOrAttack.Play();
        }
    }

    public IEnumerator DeathSequence()
    {
        spriteRenderer.sprite = deadSprite;
        if (DieAudio) DieAudio.Play();
        Time.timeScale = 0f; // Freeze the game
        yield return new WaitForSecondsRealtime(2f); // Use realtime since game is paused
        Time.timeScale = 1f; // Restore normal time
        enabled = false;
        GameManager.Instance.LevelFailed();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            currentLadder = other;
            ladderTopY = other.bounds.max.y;

            if (vInput > 0.1f)
                StartClimb(other);
            else if (grounded)
                StopClimb();

            return;
        }

        if (other.CompareTag("LadderUnderMe"))
        {
            if (grounded && vInput < -0.1f)
            {
                var ladder = FindLadderNearProbe(other);
                
                if (ladder != null)
                {
                    this.transform.position = new Vector3(transform.position.x, transform.position.y - 0.8f, transform.position.z);
                    currentLadder = ladder;
                    ladderTopY = ladder.bounds.max.y;
                    StartClimb(ladder, true);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            StopClimb();
            currentLadder = null;
            ladderTopY = 0f;
            exitingLadder = false;
        }
    }

    private void StartClimb(Collider2D ladder, bool enteringFromTopDown = false)
    {
        if (!climbing)
        {
            climbing = true;
            capsuleCollider.isTrigger = true;
            climbIndex = 0;
            if (climbSprites != null && climbSprites.Length > 0)
                spriteRenderer.sprite = climbSprites[0];
        }

        float centerX = ladder.bounds.center.x;
        float y = transform.position.y;
        if (enteringFromTopDown) y -= enterDownOffset;

        transform.position = new Vector3(centerX, y, transform.position.z);
    }

    private void StopClimb()
    {
        if (!climbing) return;
        climbing = false;
        exitingLadder = false;
        capsuleCollider.isTrigger = false;
        rb.gravityScale = 3f;
    }

    private IEnumerator ExitLadderSmooth()
    {
        if (exitingLadder) yield break;
        exitingLadder = true;
        climbing = false;
        yield return new WaitForFixedUpdate();
        capsuleCollider.isTrigger = false;
        currentLadder = null;
        ladderTopY = 0f;
        exitingLadder = false;
    }

    private Collider2D FindLadderNearProbe(Collider2D probe)
    {
        var hits = Physics2D.OverlapBoxAll(probe.bounds.center, probe.bounds.size * 7f, 0f);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag("Ladder"))
                return hits[i];
        return null;
    }
}
