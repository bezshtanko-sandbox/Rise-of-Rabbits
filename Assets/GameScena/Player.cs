using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldownTime = 0.3f;
    [SerializeField] private float deathBelowCameraMargin = 1f;

    [SyncVar] public string playerId;
    [SyncVar] public string playerName;
    [SyncVar] public bool isAlive = true;
    [SyncVar] public int characterIndex;
    [SyncVar(hook = nameof(OnFlipChanged))] private bool facingRight = true;

    public float speedMultiplier = 1f;
    public bool inverted;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isGrounded;
    private bool isIce;
    private bool canMove = true;
    private bool deathReportSent;
    private float jumpCooldown;
    private int groundContactCount;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (rb == null)
            Debug.LogError("Rigidbody2D not found on player.", this);
        if (sr == null)
            Debug.LogError("SpriteRenderer not found on player.", this);
    }

    private void Update()
    {
        if (!isLocalPlayer || !isAlive || !IsGameplayScene())
            return;

        Camera mainCamera = Camera.main;
        if (!deathReportSent && mainCamera != null && mainCamera.orthographic &&
            transform.position.y < mainCamera.transform.position.y - mainCamera.orthographicSize - deathBelowCameraMargin)
        {
            deathReportSent = true;
            CmdReportDeath();
            return;
        }

        if (!canMove || rb == null)
            return;

        float move = Input.GetAxis("Horizontal");
        if (isIce || inverted)
            move = -move;

        rb.velocity = new Vector2(move * speed * speedMultiplier, rb.velocity.y);

        if (move > 0.01f && !facingRight)
        {
            facingRight = true;
            OnFlipChanged(false, true);
            CmdSetFacing(true);
        }
        else if (move < -0.01f && facingRight)
        {
            facingRight = false;
            OnFlipChanged(true, false);
            CmdSetFacing(false);
        }

        if (jumpCooldown > 0f)
            jumpCooldown -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && jumpCooldown <= 0f)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            jumpCooldown = jumpCooldownTime;
        }
    }

    [Command]
    private void CmdSetFacing(bool value)
    {
        facingRight = value;
    }

    [Command]
    private void CmdReportDeath()
    {
        if (isAlive)
            isAlive = false;
    }

    private void OnFlipChanged(bool oldValue, bool newValue)
    {
        if (sr != null)
            sr.flipX = !newValue;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount++;
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Mirror"))
            isIce = true;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Mirror"))
            isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount--;
            if (groundContactCount <= 0)
            {
                groundContactCount = 0;
                isGrounded = false;
            }
        }

        if (collision.gameObject.CompareTag("Mirror"))
        {
            isIce = false;
            isGrounded = false;
        }
    }

    [Server]
    public void Stun(float duration)
    {
        StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    [Server]
    public void ApplyWind(Vector2 dir, float force, float duration)
    {
        RpcApplyWind(dir, force, duration);
    }

    [ClientRpc]
    private void RpcApplyWind(Vector2 dir, float force, float duration)
    {
        if (!isLocalPlayer)
            return;

        StartCoroutine(WindCoroutine(dir, force, duration));
    }

    private IEnumerator WindCoroutine(Vector2 dir, float force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rb == null)
                yield break;

            rb.AddForce(dir.normalized * force * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public void ApplySlow(float duration, float factor)
    {
        StartCoroutine(SlowCoroutine(duration, factor));
    }

    private IEnumerator SlowCoroutine(float duration, float factor)
    {
        float originalSpeed = speed;
        speed *= factor;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }


    private bool IsGameplayScene()
    {
        return SceneManager.GetActiveScene().name != "EndScena";
    }

}
