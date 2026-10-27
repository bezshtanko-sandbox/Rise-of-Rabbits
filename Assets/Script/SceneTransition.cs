using EasyTransition;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public TransitionSettings transitionSettings;

    public void TransitionToScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        TransitionManager transitionManager = TransitionManager.Instance();
        if (transitionManager != null && transitionSettings != null)
        {
            transitionManager.Transition(sceneName, transitionSettings, 0f);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
