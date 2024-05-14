using System.Collections;
using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviourSingleton<CustomSceneManager>{
    
    [SerializeField] private string startScene;
    private bool _isShared = false;
    private void Start()
    {
        Scene scene = SceneManager.GetSceneByName(startScene);
        if (!scene.isLoaded && !_isShared) LoadScene(startScene);
        _isShared = true;
    }

    /**
     * Reload the active scene
     */
    public void ReloadActiveScene() {
        Scene activeScene = SceneManager.GetActiveScene();
        LoadScene(activeScene.name);
    }

    /**
     * Load a new scene as active scene and unload active scene
     */
    public void LoadScene(string sceneName) {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.UnloadSceneAsync(activeScene);
        StartCoroutine(LoadSceneAndSetActive(sceneName));
    }

    private IEnumerator LoadSceneAndSetActive(string sceneName) {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        yield return new WaitUntil(() => asyncLoad.isDone);

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(loadedScene);
        yield return null;
    }
    
}
