using UnityEngine;

public class CameraFogShake : MonoBehaviour
{
    public float shakeAmount = 0.03f;
    public float shakeSpeed = 8f;

    private Vector3 originalPos;
    private bool active = false;

    void Start()
    {
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (!active) return;

        float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
        float offsetY = Mathf.Cos(Time.time * shakeSpeed) * shakeAmount;

        transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

    }

    public void StartShake()
    {
        active = true;
        originalPos = transform.localPosition;
    }
    public void StopShake()
    {
        active = false;
        transform.localPosition = originalPos;
    }
}
