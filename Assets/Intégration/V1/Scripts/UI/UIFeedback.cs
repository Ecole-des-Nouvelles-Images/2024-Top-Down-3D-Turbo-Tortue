using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.UI
{
    public class UIFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Color _deselectedbuttonColor;
        [SerializeField] private Color _selectedbuttonColor;
        [SerializeField] private GameObject tutoPanel;
        [SerializeField] private GameObject optionsButton;
        private static Button _currentButton;

        private void Start()
        {
            _currentButton = null;
        }

        public void OnSelect(BaseEventData eventData)
        {
            SoundManager.PlaySound(SoundType.Selected,0.3f);

            if (_currentButton != null)
            {
                _currentButton.OnDeselect(eventData);
            }

            if (GetComponent<Button>())
            {
                gameObject.GetComponent<Image>().color = _selectedbuttonColor;
                transform.DOScale(1.1f, 0.5f);
                buttonText.color = Color.white;

                if (tutoPanel)
                {
                    tutoPanel.SetActive(true);
                }

                _currentButton = GetComponent<Button>();
            }

            if (optionsButton)
            {
                if (GetComponent<Slider>())
                {
                    optionsButton.GetComponent<Image>().color = _selectedbuttonColor;
                    optionsButton.transform.DOScale(1.1f, 0.5f);
                    buttonText.color = _selectedbuttonColor;
                }

                if (GetComponent<Toggle>())
                {
                    // optionsButton.gameObject.GetComponent<Toggle>().colors. = _deselectedbuttonColor;
                    optionsButton.transform.DOScale(1.1f, 0.5f);
                    buttonText.color = _selectedbuttonColor;
                }
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (GetComponent<Button>())
            {
                gameObject.GetComponent<Image>().color = _deselectedbuttonColor;
                transform.DOScale(1.0f, 0.5f);
                buttonText.color = Color.grey;

                if (tutoPanel)
                {
                    tutoPanel.SetActive(false);
                }

                _currentButton = null;
            }

            if (optionsButton)
            {
                if (GetComponent<Slider>())
                {
                    optionsButton.gameObject.GetComponent<Image>().color = _deselectedbuttonColor;
                    optionsButton.transform.DOScale(1.0f, 0.5f);
                    buttonText.color = _deselectedbuttonColor;
                }

                if (GetComponent<Toggle>())
                {
                    // optionsButton.gameObject.GetComponent<Toggle>().colors. = _deselectedbuttonColor;
                    optionsButton.transform.DOScale(1.0f, 0.5f);
                    buttonText.color = _deselectedbuttonColor;
                }
            }
        }
        
    }
}