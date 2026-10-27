using System.Linq;
using UnityEngine;

public class Cameracs : MonoBehaviour
{
    public float smoothSpeed = 0.2f;
    private float highestY;

    void Start()
    {
        highestY = transform.position.y;
    }

    void LateUpdate()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players == null || players.Length == 0) return;
        Transform highestPlayer = players.OrderByDescending(p => p.transform.position.y).First().transform;
        if (highestPlayer.position.y > highestY)
        {
            highestY = highestPlayer.position.y;
        }
        Vector3 targetPos = new Vector3(transform.position.x, highestY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed);
    }
}
