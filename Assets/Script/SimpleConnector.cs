using UnityEngine;
using Mirror;
using TMPro;
using EasyTransition;

public class SimpleConnector : NetworkBehaviour
{

    [Header("UI")]
    public TMP_InputField ipField;

    [Header("Easy Transition")]
    public TransitionSettings transitionSettings;
    public float transitionStartDelay = 0f;

    private bool isConnecting;
    private bool isHost;

    public void HostGame()
    {
        if (isConnecting)
            return;

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        isHost = true;
        StartTransition();
    }

    public void ClientGame()
    {
        if (isConnecting)
            return;

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        if (ipField == null || string.IsNullOrWhiteSpace(ipField.text))
        {
            Debug.LogWarning("Enter the server IP address!");
            return;
        }

        NetworkManager.singleton.networkAddress = ipField.text.Trim();

        isHost = false;
        StartTransition();
    }

    private void StartTransition()
    {
        TransitionManager manager = TransitionManager.Instance();

        if (transitionSettings == null || manager == null)
        {
            StartConnection();
            return;
        }

        isConnecting = true;

        manager.onTransitionCutPointReached += StartConnection;
        manager.onTransitionEnd += RemoveTransitionListeners;

        manager.Transition(transitionSettings, transitionStartDelay);
    }

    private void StartConnection()
    {
        TransitionManager manager = TransitionManager.Instance();

        if (manager != null)
            manager.onTransitionCutPointReached -= StartConnection;

        if (NetworkManager.singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            RemoveTransitionListeners();
            return;
        }

        if (isHost)
        {
            Debug.Log("Starting Host...");
            NetworkManager.singleton.StartHost();
        }
        else
        {
            Debug.Log("Connecting to " + NetworkManager.singleton.networkAddress);
            NetworkManager.singleton.StartClient();
        }
    }

    private void RemoveTransitionListeners()
    {
        TransitionManager manager = TransitionManager.Instance();

        if (manager != null)
        {
            manager.onTransitionCutPointReached -= StartConnection;
            manager.onTransitionEnd -= RemoveTransitionListeners;
        }
    }
}
