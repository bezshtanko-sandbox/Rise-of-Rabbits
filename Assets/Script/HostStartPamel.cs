using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class HostStartPamel : MonoBehaviour
{
    public GameObject startButton;
    public GameObject playersPanel;
    public Text playersCountText;

    void Update()
    {
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            startButton.SetActive(true);
            playersPanel.SetActive(true);
        }
        else if (NetworkClient.isConnected && !NetworkServer.active)
        {
            startButton.SetActive(false);
            playersPanel.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
            playersPanel.SetActive(false);
        }
        playersCountText.text = "Гравців у лобі: " + NetworkManager.singleton.numPlayers;
    }

    public void StartGame()
    {
        startButton.SetActive(false);
        playersPanel.SetActive(false);
        playersCountText.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

}
