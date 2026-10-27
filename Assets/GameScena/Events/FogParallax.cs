using UnityEngine;

public class FogParallax : MonoBehaviour
{
    public float parallaxFactor = 0.1f;
    private Vector3 lastCameraPosition;

    void Start()
    {
        lastCameraPosition = Camera.main.transform.position;
    }

    void Update()
    {
        Vector3 cameraDelta = Camera.main.transform.position - lastCameraPosition;
        transform.position += new Vector3(cameraDelta.x * parallaxFactor, cameraDelta.y * parallaxFactor, 0);
        lastCameraPosition = Camera.main.transform.position;
    }
}
