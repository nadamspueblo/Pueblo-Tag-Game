using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualsLoader : MonoBehaviour
{
    [Tooltip("The name of the scene containing the Unity Terrain and decorative objects.")]
    public string visualSceneName = "Level_Terrain_Visuals";

    void Start()
    {
        // Only load if it's not already loaded (prevents duplicates in Editor)
        Scene scene = SceneManager.GetSceneByName(visualSceneName);
        if (!scene.isLoaded)
        {
            SceneManager.LoadSceneAsync(visualSceneName, LoadSceneMode.Additive);
        }
    }
}