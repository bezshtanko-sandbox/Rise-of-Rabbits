using Mirror;
using UnityEngine;

public class MyNetworkManager : NetworkManager
{
    public GameObject heroPrefabA;
    public GameObject heroPrefabB;
    public GameObject heroPrefabC;
    public GameObject heroPrefabD;

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        int characterIndex = Mathf.Clamp(numPlayers, 0, 3);
        GameObject prefabToSpawn = GetCharacterPrefab(characterIndex);
        if (prefabToSpawn == null)
            prefabToSpawn = playerPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("No player prefab is assigned to MyNetworkManager.");
            return;
        }

        Transform startPosition = GetStartPosition();
        GameObject playerObject = startPosition != null
            ? Instantiate(prefabToSpawn, startPosition.position, startPosition.rotation)
            : Instantiate(prefabToSpawn);

        Player player = playerObject.GetComponent<Player>();
        if (player != null)
        {
            player.playerId = conn.connectionId.ToString();
            player.playerName = "Гравець " + (characterIndex + 1);
            player.characterIndex = characterIndex;
            player.isAlive = true;
        }

        NetworkServer.AddPlayerForConnection(conn, playerObject);
    }

    private GameObject GetCharacterPrefab(int characterIndex)
    {
        switch (characterIndex)
        {
            case 0: return heroPrefabA;
            case 1: return heroPrefabB;
            case 2: return heroPrefabC;
            case 3: return heroPrefabD;
            default: return playerPrefab;
        }
    }
}
