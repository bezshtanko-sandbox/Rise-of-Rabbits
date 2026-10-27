using UnityEngine;

public class FogFloat : MonoBehaviour
{
    public float amplitude = 0.5f;
    public float frequency = 1f;
    private Vector3 localStartPos;

    void Start()
    {
        localStartPos = transform.localPosition;
    }

    void Update()
    {
        float yOffset = amplitude * Mathf.Sin(Time.time * frequency);
        transform.localPosition = localStartPos + new Vector3(0, yOffset, 0);
    }

    void OnDestroy()
    {
        transform.localPosition = localStartPos;
    }
}
