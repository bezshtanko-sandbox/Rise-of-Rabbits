using UnityEngine;
using UnityEngine.UI;

public class playerMenu : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 6f;

    [Header("Start Button")]
    public Button startButton;
    public RectTransform buttonRect;
    public RectTransform canvasRect;
    public int clicksToStart = 3;

    private Rigidbody2D rb;
    private int clickCount = 0;
    public bool startGame = false;
    private bool isJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startGame = false;
    }

    void FixedUpdate()
    {
        if (!startGame || rb == null)
            return;

        rb.velocity = new Vector2(speed, rb.velocity.y);
    }

    public void StartGame()
    {
        if (startGame)
            return;

        clickCount++;

        if (clickCount < clicksToStart)
        {
            MoveButton();
        }
        else
        {
            startGame = true;

            if (startButton != null)
                startButton.gameObject.SetActive(false);

            Debug.Log("Персонаж побежал!");
        }
    }

    private void MoveButton()
    {
        if (buttonRect == null || canvasRect == null)
            return;

        float halfWidth = canvasRect.rect.width / 2f;
        float halfHeight = canvasRect.rect.height / 2f;

        float buttonHalfWidth =
            buttonRect.rect.width * buttonRect.localScale.x / 2f;
        float buttonHalfHeight =
            buttonRect.rect.height * buttonRect.localScale.y / 2f;

        float x = Random.Range(
            -halfWidth + buttonHalfWidth,
            halfWidth - buttonHalfWidth
        );

        float y = Random.Range(
            -halfHeight + buttonHalfHeight,
            halfHeight - buttonHalfHeight
        );

        buttonRect.anchoredPosition = new Vector2(x, y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Jump") && !isJumping)
        {
            isJumping = true;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        isJumping = false;
    }
}
