using Mirror;
using UnityEngine;

public class EndPlatformSpawner : NetworkBehaviour
{
    public GameObject endPlatformPrefab;
    public float yoffset = 2f;

    public override void OnStartServer()
    {
        int playerCount = NetworkManager.singleton.numPlayers;

        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        float spacing = camWidth / (playerCount + 1);

        for (int i = 0; i < playerCount; i++)
        {
            float xPos = -camWidth / 2 + spacing * (i + 1);
            Vector3 spawnPosition = new Vector3(xPos, yoffset, 0f);
            GameObject endPlatform = Instantiate(endPlatformPrefab, spawnPosition, Quaternion.identity);
            NetworkServer.Spawn(endPlatform);
        }
    }

}
