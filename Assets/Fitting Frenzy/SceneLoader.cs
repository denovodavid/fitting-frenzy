using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "SceneLoader", menuName = "Fitting Frenzy/Scene Loader", order = 0)]
public class SceneLoader : ScriptableObject
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
        // Do not allow quit from web player.
#else
        Application.Quit();
#endif
    }
}
