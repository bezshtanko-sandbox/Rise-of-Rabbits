using UnityEngine;

public class GameUI : MonoBehaviour
{
    public GameObject startGamePanel;

    public void HideStartPanel()
    {
        if (startGamePanel != null)
            startGamePanel.SetActive(false);
    }
}
