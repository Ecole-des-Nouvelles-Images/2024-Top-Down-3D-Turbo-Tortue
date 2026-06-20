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
        private Scene _activeScene;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
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
            LoadScene(activeScene.name,true);

        }

        /**
     * Load a new scene as active scene and unload active scene
     */
        public void LoadScene(string sceneName, bool showLoading = false)
        {
            DOTween.KillAll();
            _activeScene = SceneManager.GetActiveScene();
            SceneManager.UnloadSceneAsync(_activeScene);
            
            /*if (_firstTimeLoad)
            {
                _firstTimeLoad = false;
                _activeScene = SceneManager.GetActiveScene();
                StartCoroutine(LoadSceneAndSetActive(sceneName, false));
                return;
            }*/
          
            
            if (showLoading)
            {
                DataManager.Instance.loadingScreen.SetActive(true);
                StartCoroutine(LoadSceneAndSetActive(sceneName, true));
                
            }
            else
            {
                StartCoroutine(LoadSceneAndSetActive(sceneName, false));
            }

        }


        private IEnumerator LoadSceneAndSetActive(string sceneName, bool showLoading)
        {
          
            // Faux chargement
            if (showLoading) { yield return new WaitForSeconds(3f);}
          
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            yield return new WaitUntil(() => asyncLoad.isDone);

            
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(loadedScene);
            DataManager.Instance.loadingScreen.SetActive(false);
           
        }



    }


}