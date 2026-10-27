using Mirror;
using UnityEngine;
using System.Collections;
using TMPro;

public class EventManager : NetworkBehaviour
{
    public string[] eventTypes = new string[]
      {
        "Ice",
        "Invert",
        "Speed",
        "SuperWind",
        "Magnet",
        "GravityJump",
        "Rain",
        "Psycho"
      };

    public PhysicsMaterial2D iceMat;
    public PhysicsMaterial2D normalMat;
    public bool ifStarted = false;

    [Header("Event Message")]
    public TMP_Text eventMessageText;
    public float messageDuration = 4f;

    private Coroutine messageCoroutine;


    public void Started()
    {

        ifStarted = true;
        if (isServer)
        {
            StartCoroutine(EventCoroutine());
        }
    }

    IEnumerator EventCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));

            int eventType = Random.Range(0, eventTypes.Length);

            switch (eventType)
            {
                case 0: RpcStartIceEvent(); break;
                case 1: RpcStartInvertedEvent(); break;
                case 2: RpcStartSpeedEvent(); break;
                case 3: RpcStartSuperWindEvent(); break;
                case 4: RpcStartMagnetEvent(); break;
                case 5: RpcStartGravityJumpEvent(); break;
                case 6: RpcStartRainEvent(); break;
                case 7: RpcStartPsychoEvent(); break;
            }
        }
    }

    [ClientRpc]
    void RpcStartIceEvent()
    {
        ShowEventMessage("ICE EVENT! The ground is slippery!");
        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            if (col.CompareTag("Ground"))
                col.sharedMaterial = iceMat;
        }

        StartCoroutine(ResetIce(5f));
    }

    IEnumerator ResetIce(float t)
    {
        yield return new WaitForSeconds(t);

        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            if (col.CompareTag("Ground"))
                col.sharedMaterial = normalMat;
        }
    }

    [ClientRpc]
    void RpcStartInvertedEvent()
    {
        ShowEventMessage("INVERTED EVENT! The controls are reversed!");
        foreach (var p in FindObjectsOfType<Player>())
            p.inverted = true;

        StartCoroutine(ResetInvert(3f));
    }

    IEnumerator ResetInvert(float t)
    {
        yield return new WaitForSeconds(t);

        foreach (var p in FindObjectsOfType<Player>())
            p.inverted = false;
    }

    [ClientRpc]
    void RpcStartSpeedEvent()
    {
        ShowEventMessage("SPEED EVENT! Double speed!");
        foreach (var p in FindObjectsOfType<Player>())
            p.speedMultiplier = 4f;

        StartCoroutine(ResetSpeed(3f));
    }

    IEnumerator ResetSpeed(float t)
    {
        yield return new WaitForSeconds(t);

        foreach (var p in FindObjectsOfType<Player>())
            p.speedMultiplier = 1f;
    }

    [ClientRpc]
    void RpcStartSuperWindEvent()
    {
        ShowEventMessage("SUPER WIND EVENT! A strong wind is blowing!");
        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            if (rb.CompareTag("Player"))
                rb.AddForce(Vector2.right * 80f, ForceMode2D.Impulse);
        }
    }

    [ClientRpc]
    void RpcStartMagnetEvent()
    {
        ShowEventMessage("MAGNET EVENT! Players are pulled together!");
        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 center = cam.transform.position;

        foreach (var rb in FindObjectsOfType<Rigidbody2D>())
        {
            if (rb.CompareTag("Player"))
            {
                Vector2 dir = (center - rb.transform.position).normalized;
                rb.AddForce(dir * 50f, ForceMode2D.Impulse);
            }
        }
    }

    [ClientRpc]
    void RpcStartGravityJumpEvent()
    {
        ShowEventMessage("LOW GRAVITY! Jump higher!");
        Physics2D.gravity = new Vector2(0, -1f);
        StartCoroutine(ResetGravity(1f));
    }

    IEnumerator ResetGravity(float t)
    {
        yield return new WaitForSeconds(t);
        Physics2D.gravity = new Vector2(0, -9.81f);
    }

    public GameObject rainPrefab;

    [ClientRpc]
    void RpcStartRainEvent()
    {
        ShowEventMessage("RAIN EVENT! Watch your step!");
        Camera cam = Camera.main;
        if (!cam) return;

        Vector3 pos = cam.transform.position + new Vector3(0, cam.orthographicSize + 1f, 0);

        GameObject rain = Instantiate(rainPrefab, pos, Quaternion.identity);
        rain.transform.SetParent(cam.transform);

        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            if (col.CompareTag("Ground"))
                col.sharedMaterial = iceMat;
        }

        StartCoroutine(ResetRain(5f));
        Destroy(rain, 5f);
    }

    IEnumerator ResetRain(float t)
    {
        yield return new WaitForSeconds(t);

        foreach (var col in FindObjectsOfType<Collider2D>())
        {
            if (col.CompareTag("Ground"))
                col.sharedMaterial = normalMat;
        }
    }

    [ClientRpc]
    void RpcStartPsychoEvent()
    {
        ShowEventMessage("PSYCHO EVENT! Reality is distorted!");
        Camera cam = Camera.main;
        if (!cam) return;

        float duration = 3f;

        StartCoroutine(PsychoWave(cam, duration));
        StartCoroutine(PsychoPulse(cam, duration));
    }

    IEnumerator PsychoWave(Camera cam, float duration)
    {
        float t = 0f;
        Vector3 originalScale = cam.transform.localScale;

        while (t < duration)
        {
            t += Time.deltaTime;

            float wave = Mathf.Sin(Time.time * 6f) * 0.1f;

            cam.transform.localScale = new Vector3(
                originalScale.x + wave,
                originalScale.y - wave,
                originalScale.z
            );

            yield return null;
        }

        cam.transform.localScale = originalScale;
    }

    IEnumerator PsychoPulse(Camera cam, float duration)
    {
        float t = 0f;
        float baseSize = cam.orthographicSize;

        while (t < duration)
        {
            t += Time.deltaTime;

            cam.orthographicSize = baseSize + Mathf.Sin(Time.time * 5f) * 0.4f;

            yield return null;
        }

        cam.orthographicSize = baseSize;
    }

    void ShowEventMessage(string message)
    {
        if (eventMessageText == null) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }

    IEnumerator ShowMessageCoroutine(string message)
    {
        eventMessageText.text = message;
        eventMessageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        eventMessageText.gameObject.SetActive(false);
        messageCoroutine = null;
    }

}
