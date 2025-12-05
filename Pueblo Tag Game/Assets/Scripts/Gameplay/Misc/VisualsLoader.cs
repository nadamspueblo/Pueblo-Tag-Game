using System.Collections.Generic; // Added for Lists
using UnityEngine;
using UnityEngine.SceneManagement;

public class VisualsLoader : MonoBehaviour
{
    [Tooltip("List of standard scenes to load additively (UI, Terrain Visuals, etc.)")]
    public List<string> scenesToLoad = new List<string>();

    void Start()
    {
        foreach (var sceneName in scenesToLoad)
        {
            if (string.IsNullOrEmpty(sceneName)) continue;

            // Only load if it's not already loaded
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }
        }
    }
}