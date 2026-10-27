using UnityEngine;
using Mirror;

public class ManagerGameEnd : NetworkBehaviour
{

    private GameObject losePanel;
    Transform player;

    void Start()
    {
        player = transform;
        losePanel = GameObject.FindGameObjectWithTag("GameEndPanel");
        if (isLocalPlayer && losePanel != null)
            losePanel.SetActive(false);
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        float cameraY = Camera.main.transform.position.y;
        float bottomY = cameraY - Camera.main.orthographicSize;

        if (player.position.y < bottomY)
        {
            CmdKillMe();
        }
    }

    [Command]
    void CmdKillMe()
    {
        TargetShowLosePanel(connectionToClient);
        NetworkServer.Destroy(gameObject);
    }

    [TargetRpc]
    void TargetShowLosePanel(NetworkConnection target)
    {
        if (losePanel != null)
            losePanel.SetActive(true);
    }

}
