using System.Collections;
using DG.Tweening;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

namespace Intégration.V1.Scripts.SharedScene
{
    public class CustomSceneManager : MonoBehaviourSingleton<CustomSceneManager>
    {
        [SerializeField] private string startScene;
        private bool _isShared = false;
        private bool _firstTimeLoad = true;

        private void Start()
        {
            Cursor.lockState =CursorLockMode.Locked;
            Cursor.visible = false;
            
            Scene scene = SceneManager.GetSceneByName(startScene);
            if (!scene.isLoaded && !_isShared) LoadScene(startScene);
            _isShared = true;
        }


        /**
     * Reload the active scene
     */
        public void ReloadActiveScene()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            LoadScene(activeScene.name);
            
        }

        /**
     * Load a new scene as active scene and unload active scene
     */
        public void LoadScene(string sceneName)
        {
            DOTween.KillAll();
            if (!_firstTimeLoad) { DataManager.Instance.loadingScreen.SetActive(true); }
           
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.UnloadSceneAsync(activeScene);
            StartCoroutine(LoadSceneAndSetActive(sceneName));
            _firstTimeLoad = false;
            
            foreach (var user in InputUser.all)
            {
                Debug.Log("user dispo : " +user + "numero : " + user.index);
            }
        }


        private IEnumerator LoadSceneAndSetActive(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);


            //DataManager.Instance.loadingScreen.SetActive(true);
            yield return new WaitUntil(() => asyncLoad.isDone); // Wait until the asynchronous scene fully loads

            DataManager.Instance.loadingScreen.SetActive(false);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
            yield return null;
        }
    }
    
    
    
    
    
    
}