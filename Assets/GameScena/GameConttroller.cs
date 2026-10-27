using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using EasyTransition;

public class GameConttroller : NetworkBehaviour
{
    [Header("Game Settings")]
    public float timeLimit = 300f;

    [SyncVar] private float timer;
    [SyncVar(hook = nameof(OnGameStartedChanged))]
    private bool gameStarted;

    private bool gameEnded;
    private int startingPlayerCount;

    [Header("Lobby UI")]
    public Button startButton;
    public Text playersCountText;
    public GameObject StartGamePanel;

    [Header("Game UI")]
    public Text timerText;
    [Header("Easy Transition")]
    public TransitionSettings endSceneTransition;
    public float transitionStartDelay = 0f;

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (timerText != null)
            timerText.text = string.Empty;

        RefreshLobbyUI();
    }

    private void Update()
    {
        RefreshLobbyUI();

        if (playersCountText != null &&
            NetworkManager.singleton != null)
        {
            playersCountText.text =
                "Гравців у лобі: " +
                NetworkManager.singleton.numPlayers;
        }

        if (!gameStarted || gameEnded)
            return;

        if (isServer)
        {
            timer += Time.deltaTime;

            if (timer >= timeLimit)
            {
                EndGame("time");
                return;
            }

            CheckAlivePlayers();
        }

        if (timerText != null)
        {
            timerText.text =
                Mathf.Max(0, Mathf.CeilToInt(timeLimit - timer))
                .ToString();
        }
    }

    private void RefreshLobbyUI()
    {
        bool connected = NetworkClient.isConnected;
        bool showLobby = connected && !gameStarted;
        bool canStart = showLobby && NetworkServer.active;

        if (startButton != null)
            startButton.gameObject.SetActive(canStart);

        if (playersCountText != null)
            playersCountText.gameObject.SetActive(showLobby);

        if (StartGamePanel != null)
            StartGamePanel.SetActive(showLobby);
    }

    private void OnGameStartedChanged(bool oldValue, bool newValue)
    {
        RefreshLobbyUI();
    }

    private void OnStartButtonClicked()
    {
        if (!isServer || gameEnded || gameStarted)
            return;

        Player[] players = FindObjectsOfType<Player>();
        startingPlayerCount = players.Length;

        timer = 0f;
        gameStarted = true;
    }

    [Server]
    private void CheckAlivePlayers()
    {
        Player[] players = FindObjectsOfType<Player>();

        if (players.Length == 0)
            return;

        int aliveCount = 0;

        foreach (Player player in players)
        {
            if (player.isAlive)
                aliveCount++;
        }

        if (aliveCount == 0)
            EndGame("allDead");
        else if (aliveCount == 1 && startingPlayerCount > 1)
            EndGame("lastAlive");
    }

    [ClientRpc]
    private void RpcStartEndTransition()
    {
        if (endSceneTransition == null)
        {
            Debug.LogWarning("End Scene Transition is not assigned.");
            return;
        }

        TransitionManager manager = TransitionManager.Instance();

        if (manager == null)
        {
            Debug.LogWarning("TransitionManager not found.");
            return;
        }

        manager.Transition(endSceneTransition, transitionStartDelay);
    }

    [Server]
    private void EndGame(string reason)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        List<PlayerResultData> results = new List<PlayerResultData>();

        foreach (Player player in FindObjectsOfType<Player>())
        {
            results.Add(new PlayerResultData
            {
                playerId = player.playerId,
                playerName = player.playerName,
                isAlive = player.isAlive,
                characterIndex = player.characterIndex
            });
        }

        GameResult.Set(reason, results);
        RpcSetResults(GameResult.ToJson());

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("NetworkManager is missing!", this);
            return;
        }

        if (endSceneTransition != null)
            RpcStartEndTransition();

        StartCoroutine(ChangeToEndScene());
    }

    [ClientRpc]
    private void RpcSetResults(string serializedResults)
    {
        GameResult.LoadJson(serializedResults);
    }

    public override void OnStopServer()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);

        base.OnStopServer();
    }
    [Server]
    private System.Collections.IEnumerator ChangeToEndScene()
    {
        float delay = transitionStartDelay;

        if (endSceneTransition != null)
        {
            delay += endSceneTransition.transitionTime;

            if (endSceneTransition.autoAdjustTransitionTime)
            {
                delay = transitionStartDelay +
                    endSceneTransition.transitionTime /
                    Mathf.Max(0.01f, endSceneTransition.transitionSpeed);
            }
        }

        yield return new WaitForSecondsRealtime(delay);

        if (NetworkManager.singleton != null)
            NetworkManager.singleton.ServerChangeScene("EndScena");
    }
}

[System.Serializable]
public class PlayerResultData
{
    public string playerId;
    public string playerName;
    public bool isAlive;
    public int characterIndex;
}

[System.Serializable]
public class GameResultSnapshot
{
    public string reason;
    public List<PlayerResultData> players =
        new List<PlayerResultData>();
}

public static class GameResult
{
    public static string reason;
    public static List<PlayerResultData> players =
        new List<PlayerResultData>();

    public static void Set(
        string resultReason,
        List<PlayerResultData> results)
    {
        reason = resultReason;
        players = results ?? new List<PlayerResultData>();
    }

    public static string ToJson()
    {
        return JsonUtility.ToJson(
            new GameResultSnapshot
            {
                reason = reason,
                players = players
            }
        );
    }

    public static void LoadJson(string serializedResults)
    {
        if (string.IsNullOrEmpty(serializedResults))
        {
            Set(string.Empty, null);
            return;
        }

        GameResultSnapshot snapshot =
            JsonUtility.FromJson<GameResultSnapshot>(
                serializedResults
            );

        Set(
            snapshot != null ? snapshot.reason : string.Empty,
            snapshot != null ? snapshot.players : null
        );
    }
}

