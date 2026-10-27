using Mirror;
using UnityEngine;

public class PlatfromMoving : NetworkBehaviour
{

    public float distance = 2f;
    public float obstacleCheckDistance = 0.3f;
    [SyncVar] private float speed;
    [SyncVar] private bool canMove;

    private Vector3 leftPoint;
    private Vector3 rightPoint;
    private Vector3 target;

    void Start()
    {
        leftPoint = transform.position + Vector3.left * distance;
        rightPoint = transform.position + Vector3.right * distance;

        if (isServer)
        {
            speed = Random.Range(0.1f, 1f);
            canMove = Random.value > 0.5f;
            target = Random.Range(0, 2) == 0 ? leftPoint : rightPoint;
        }
    }

    [ServerCallback]
    void Update()
    {
        if (!canMove) return;

        Vector2 direction = (target - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance);

        if (hit.collider != null && hit.collider.CompareTag("Ground") && hit.collider.gameObject.CompareTag("Mirror"))
        {
            target = target == leftPoint ? rightPoint : leftPoint;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            target = target == leftPoint ? rightPoint : leftPoint;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (target - transform.position).normalized * obstacleCheckDistance);
    }
}
