using Intégration.V1.Scripts.SharedScene;
using UnityEngine;
using UnityEngine.Video;

namespace Intégration.V1.Scripts.IntroScreen
{
    public class LoadMenu : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayerIntro;

        private void Start()
        {
            _videoPlayerIntro.loopPointReached += LoadMenuScene;
        }

        public void LoadMenuScene(VideoPlayer videoPlayer)
        {
            CustomSceneManager.Instance.LoadScene("Menu");
        }
    }
}