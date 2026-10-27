using UnityEngine;
using UnityEngine.UI;

public class ExitButton : MonoBehaviour
{

    public void ExitGame()
    {
        Debug.Log("Гравець вийшов з гри!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}


