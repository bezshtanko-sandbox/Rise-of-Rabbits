using UnityEngine;

public class StartGame : MonoBehaviour
{
    public GameObject panel;

    void Awake()
    {
        panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            panel.SetActive(true);
            Destroy(collision.gameObject);
        }
    }
}