using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Menu
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MultiplayerEventSystem _multiplayerEventSystem;
        [SerializeField] private GameObject _previousPanel;
        [SerializeField] private GameObject _previousSelected;
        
        private void OnEnable()
        {
            InputManager.OnBackPressed += HandleBack;
        }

        private void OnDisable()
        {
            InputManager.OnBackPressed -= HandleBack;
        }


        private void HandleBack()
        {
            if (!gameObject.activeInHierarchy) return;
            if (!_previousPanel && !_multiplayerEventSystem) return;
           
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonCanceled);
            _previousPanel.SetActive(true);
            _multiplayerEventSystem.SetSelectedGameObject(_previousSelected);
            gameObject.SetActive(false);
        }
    }
}