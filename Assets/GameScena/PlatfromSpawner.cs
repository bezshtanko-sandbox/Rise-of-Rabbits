using Mirror;
using UnityEngine;

public class PlatfromSpawner : NetworkBehaviour
{
    public GameObject platformPrefab;
    public GameObject[] specialPlatformPrefabs;
    public float spawnY = 2f;
    public float cameraWidth = 3f;
    public float despawnOffset = 3.4f;
    public float maxSpawnHeightAbovePlayer = 20f;
    public float minDistanceBetweenPlatforms = 2f;
    public float specialPlatformChance = 0.1f;
    public float minDistanceX = 2f;

    private Transform playerTransform;
    private Transform firstPlatform;
    private float nextY;
    bool IsPlaceFree(Vector3 pos)
    {

        Vector2 size = new Vector2(1f, 1f);
        Collider2D hit = Physics2D.OverlapBox(pos, size, 0f);
        return hit == null;
    }

    void Start()
    {
        if (!isServer) return;
        firstPlatform = GameObject.FindWithTag("Ground")?.transform;
        nextY = (firstPlatform != null ? firstPlatform.position.y : transform.position.y) + minDistanceBetweenPlatforms;
        InvokeRepeating(nameof(SpawnPlatform), 0f, 0.1f);
    }


    [Server]
    void SpawnPlatform()
    {
        Player[] players = FindObjectsOfType<Player>();

        // Генеруємо платформи з урахуванням найвищого гравця.
        if (players.Length > 0)
        {
            float highestPlayerY = float.MinValue;

            foreach (Player player in players)
            {
                if (player != null)
                    highestPlayerY = Mathf.Max(
                        highestPlayerY,
                        player.transform.position.y
                    );
            }

            if (nextY > highestPlayerY + maxSpawnHeightAbovePlayer)
                return;
        }

        float spawnMinX = -cameraWidth;
        float spawnMaxX = cameraWidth;

        int platformCount = 4;
        float sectorWidth = (spawnMaxX - spawnMinX) / platformCount;

        for (int i = 0; i < platformCount; i++)
        {
            float sectorMin = spawnMinX + i * sectorWidth;
            float sectorMax = sectorMin + sectorWidth;

            float x = Random.Range(
                sectorMin + 0.3f,
                sectorMax - 0.3f
            );

            GameObject prefabToUse = platformPrefab;

            if (specialPlatformPrefabs != null &&
                specialPlatformPrefabs.Length > 0 &&
                Random.value < specialPlatformChance)
            {
                int index = Random.Range(
                    0,
                    specialPlatformPrefabs.Length
                );

                prefabToUse = specialPlatformPrefabs[index];
            }

            if (prefabToUse == null)
                continue;

            GameObject platform = Instantiate(
                prefabToUse,
                new Vector3(x, nextY, 0f),
                Quaternion.identity
            );

            NetworkServer.Spawn(platform);
        }

        nextY += minDistanceBetweenPlatforms;
    }

    void Update()
    {
        if (!isServer)
            return;

        Player[] players = FindObjectsOfType<Player>();

        // Не видаляємо платформи, поки немає гравців.
        if (players.Length == 0)
            return;

        // Найнижчий гравець визначає, які платформи
        // вже можна безпечно видалити.
        float lowestPlayerY = float.MaxValue;

        foreach (Player player in players)
        {
            if (player != null)
            {
                lowestPlayerY = Mathf.Min(
                    lowestPlayerY,
                    player.transform.position.y
                );
            }
        }

        float destroyBelowY = lowestPlayerY - despawnOffset;

        foreach (GameObject platform in
                 GameObject.FindGameObjectsWithTag("Ground"))
        {
            if (platform != null &&
                platform.transform.position.y < destroyBelowY)
            {
                NetworkServer.Destroy(platform);
            }
        }

        foreach (GameObject platform in
                 GameObject.FindGameObjectsWithTag("Mirror"))
        {
            if (platform != null &&
                platform.transform.position.y < destroyBelowY)
            {
                NetworkServer.Destroy(platform);
            }
        }
    }

}
