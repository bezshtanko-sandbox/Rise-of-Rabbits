using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EasyTransition;


public class EndScenaManager : MonoBehaviour
{
    [Header("Easy Transition")]
    public TransitionSettings menuTransitionSettings;
    public float transitionStartDelay = 0f;
    public string menuSceneName = "MenuScene";
    private bool isQuitting;
    [Header("Platform Settings")]
    public GameObject platformPrefab;
    public float platformY = -2f;
    public float totalWidth = 16f;

    [Header("Characters")]
    public GameObject[] characterPrefabs;
    public float avatarYOffset = 1.3f;

    [Header("Status Text")]
    public GameObject statusTextPrefab;
    public float statusYOffset = 2.3f;

    [Header("UI")]
    public Text titleText;
    public Text resultText;
    public Button quitButton;

    private Canvas canvas;
    private Camera sceneCamera;
    private bool missingStatusPrefabLogged;
    List<PlayerResultData> results = GameResult.players;

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        sceneCamera = Camera.main;

        if (canvas == null || sceneCamera == null)
        {
            Debug.LogError("End scene needs a Canvas and a camera tagged MainCamera.", this);
            return;
        }

        ShowResults();
        SpawnEverything();

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    private void SpawnEverything()
    {
        int count = GameResult.players == null ? 0 : Mathf.Clamp(GameResult.players.Count, 0, 4);
        if (count == 0 || platformPrefab == null)
            return;

        SpriteRenderer platformRenderer = platformPrefab.GetComponentInChildren<SpriteRenderer>(true);
        if (platformRenderer == null || platformRenderer.sprite == null)
        {
            Debug.LogWarning("The podium prefab needs a SpriteRenderer with a sprite.", this);
            return;
        }

        List<Vector3> positions = GetPlatformPositions(count);
        for (int i = 0; i < count; i++)
        {
            Vector3 position = positions[i];
            Instantiate(platformPrefab, position, Quaternion.identity);

            PlayerResultData data = GameResult.players[i];
            if (characterPrefabs != null && data.characterIndex >= 0 && data.characterIndex < characterPrefabs.Length)
            {
                GameObject characterPrefab = characterPrefabs[data.characterIndex];
                if (characterPrefab != null)
                {
                    SpriteRenderer characterRenderer = characterPrefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (characterRenderer != null && characterRenderer.sprite != null)
                    {
                        Instantiate(platformPrefab, position, Quaternion.identity);
                    }
                }
            }

            AddStatusLabel(data, position + Vector3.up * statusYOffset);
        }
    }

    private Vector2 GetWorldSize(SpriteRenderer spriteRenderer)
    {
        Vector3 scale = spriteRenderer.transform.lossyScale;
        return new Vector2(
            spriteRenderer.sprite.bounds.size.x * Mathf.Abs(scale.x),
            spriteRenderer.sprite.bounds.size.y * Mathf.Abs(scale.y));
    }

    private void AddSpriteToCanvas(Sprite sprite, Vector3 worldPosition, Vector2 worldSize, string objectName)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(canvas.transform, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        SetRectFromWorld(rect, worldPosition, worldSize);
    }

    private void AddStatusLabel(PlayerResultData data, Vector3 worldPosition)
    {
        GameObject label;
        if (statusTextPrefab != null)
        {
            label = Instantiate(statusTextPrefab, canvas.transform, false);
        }
        else
        {
            if (!missingStatusPrefabLogged)
            {
                Debug.LogWarning("Status text prefab is not assigned; using a built-in text label.", this);
                missingStatusPrefabLogged = true;
            }

            label = new GameObject("Player Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Text fallbackText = label.GetComponent<Text>();
            fallbackText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            fallbackText.fontSize = 18;
            fallbackText.alignment = TextAnchor.MiddleCenter;
            fallbackText.color = Color.white;
            fallbackText.raycastTarget = false;
        }

        Text statusText = label.GetComponentInChildren<Text>(true);
        if (statusText != null)
        {
            string playerName = string.IsNullOrWhiteSpace(data.playerName) ? "Гравець" : data.playerName;
            statusText.text = playerName + "\n" + (data.isAlive ? "Alive" : "Dead");
            statusText.color = data.isAlive ? Color.green : Color.red;
            statusText.fontSize = 18;
            statusText.alignment = TextAnchor.MiddleCenter;
            statusText.raycastTarget = false;
        }

        RectTransform rect = label.GetComponent<RectTransform>();
        if (rect != null)
            SetRectFromWorld(rect, worldPosition, new Vector2(3.2f, 0.9f));
    }

    private void SetRectFromWorld(RectTransform rect, Vector3 worldPosition, Vector2 worldSize)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;

        RectTransform canvasRect = canvas.transform as RectTransform;
        Camera eventCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector3 screenCenter = sceneCamera.WorldToScreenPoint(worldPosition);
        Vector2 localPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenCenter, eventCamera, out localPosition))
            rect.anchoredPosition = localPosition;

        Vector3 screenMin = sceneCamera.WorldToScreenPoint(worldPosition - new Vector3(worldSize.x * 0.5f, worldSize.y * 0.5f, 0f));
        Vector3 screenMax = sceneCamera.WorldToScreenPoint(worldPosition + new Vector3(worldSize.x * 0.5f, worldSize.y * 0.5f, 0f));
        float scaleFactor = canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
        rect.sizeDelta = new Vector2(
            Mathf.Abs(screenMax.x - screenMin.x) / scaleFactor,
            Mathf.Abs(screenMax.y - screenMin.y) / scaleFactor);
    }



    private List<Vector3> GetPlatformPositions(int count)
    {
        List<Vector3> positions = new List<Vector3>();

        if (count <= 0 || sceneCamera == null)
            return positions;

        float[] xPositions;

        switch (count)
        {
            case 1:
                xPositions = new float[]
                {
                0f
                };
                break;

            case 2:
                xPositions = new float[]
                {
                -1.2f, 1.2f
                };
                break;

            case 3:
                xPositions = new float[]
                {
                -2.4f, 0f, 2.4f
                };
                break;

            case 4:
                xPositions = new float[]
                {
                -3.675f, -1.2f, 1.2f, 3.675f
                };
                break;

            default:
                return positions;
        }

        for (int i = 0; i < xPositions.Length; i++)
        {
            positions.Add(new Vector3(
                sceneCamera.transform.position.x + xPositions[i],
                platformY,
                0f
            ));
        }

        return positions;
    }



    private void ShowResults()
    {
        if (titleText != null)
        {
            switch (GameResult.reason)
            {
                case "time":
                    titleText.text = "TIME'S UP!";
                    break;

                case "allDead":
                    titleText.text = "EVERYONE IS DEAD";
                    break;

                case "lastAlive":
                    titleText.text = "VICTORY!";
                    break;

                default:
                    titleText.text = "GAME OVER";
                    break;
            }
        }

        if (resultText == null)
            return;

        if (GameResult.players == null || GameResult.players.Count == 0)
        {
            resultText.text = "No player data available.";
            return;
        }

        System.Text.StringBuilder builder =
            new System.Text.StringBuilder();

        foreach (PlayerResultData player in GameResult.players)
        {
            string playerName = string.IsNullOrWhiteSpace(player.playerName)
                ? player.playerId
                : player.playerName;

            builder.AppendLine(
                playerName + " — " +
                (player.isAlive ? "Alive" : "Dead")
            );
        }

        resultText.text = builder.ToString();
    }
    private void OnQuit()
    {
        if (isQuitting)
            return;

        if (menuTransitionSettings == null)
        {
            Debug.LogError("Menu Transition Settings is not assigned!", this);
            return;
        }

        TransitionManager transitionManager = TransitionManager.Instance();

        if (transitionManager == null)
        {
            Debug.LogError("TransitionManager was not found in the scene!", this);
            return;
        }

        isQuitting = true;

        if (quitButton != null)
            quitButton.interactable = false;

        // Останавливаем Mirror-соединение.
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
        }

        // Запускаем Easy Transition.
        transitionManager.Transition(
            menuSceneName,
            menuTransitionSettings,
            transitionStartDelay
        );
    }
}
