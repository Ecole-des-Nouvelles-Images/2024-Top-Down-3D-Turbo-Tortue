using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.UI
{
    public class UIFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler
    {
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Color _deselectedbuttonColor;
        [SerializeField] private Color _selectedbuttonColor;
        [SerializeField] private GameObject tutoPanel;
        [SerializeField] private GameObject optionsButton;
        [SerializeField] private float _bounceDuration  = 0.2f;
        private static Button _currentButton;

        private void Start()
        {
            _currentButton = null;
            
        }

        public void OnSelect(BaseEventData eventData)
        {
            //SoundManager.PlaySound(SoundType.Selected,0.3f);
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonSelected);

            if (_currentButton != null)
            {
                _currentButton.OnDeselect(eventData);
            }

            if (GetComponent<Button>())
            {
                gameObject.GetComponent<Image>().color = _selectedbuttonColor;
                transform.DOScale(1.1f, _bounceDuration).SetUpdate(true).SetEase(Ease.OutBounce);
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
                    optionsButton.transform.DOScale(1.1f, _bounceDuration).SetUpdate(true).SetEase(Ease.OutBounce);
                    buttonText.color = _selectedbuttonColor;
                }

                if (GetComponent<Toggle>())
                {
                    // optionsButton.gameObject.GetComponent<Toggle>().colors. = _deselectedbuttonColor;
                    optionsButton.transform.DOScale(1.1f, _bounceDuration).SetUpdate(true).SetEase(Ease.OutBounce);
                    buttonText.color = _selectedbuttonColor;
                }

                if (GetComponent<TMP_Dropdown>())
                {
                    optionsButton.transform.DOScale(1.1f, _bounceDuration).SetUpdate(true).SetEase(Ease.OutBounce);
                    buttonText.color = _selectedbuttonColor;
                }
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (GetComponent<Button>())
            {
                gameObject.GetComponent<Image>().color = _deselectedbuttonColor;
                transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InBounce);
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
                    optionsButton.transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InBounce);
                    buttonText.color = _deselectedbuttonColor;
                }

                if (GetComponent<Toggle>())
                {
                    // optionsButton.gameObject.GetComponent<Toggle>().colors. = _deselectedbuttonColor;
                    optionsButton.transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InBounce);
                    buttonText.color = _deselectedbuttonColor;
                }
                
                if (GetComponent<Dropdown>())
                {
                    optionsButton.transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InBounce);
                    buttonText.color = _deselectedbuttonColor;
                }
            }
        }
        

        public void OnSubmit(BaseEventData eventData)
        {
            if (optionsButton)
            {
                Debug.Log("OnPointerEnter");
                optionsButton.transform.localScale = Vector3.one;
                optionsButton.transform.DOPunchScale(Vector3.one * 0.2f, _bounceDuration).SetUpdate(true).SetEase(Ease.Linear);
            }
        }
    }
}