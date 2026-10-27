using UnityEngine;

public class fragileplatfrom : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite damagedSprite;

    private int stepCount = 0;
    private SpriteRenderer sr;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = normalSprite;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        stepCount++;

        if (stepCount == 1)
        {
            sr.sprite = damagedSprite;
        }
        else if (stepCount >= 2)
        {
            Invoke("d", 4f);
        }
    }
    void d() => Destroy(gameObject);
}
