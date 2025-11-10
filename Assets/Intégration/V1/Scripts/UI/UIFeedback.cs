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
        [SerializeField] private Color _deselectedTextColor;
        [SerializeField] private Color _selectedTextColor = Color.white;
        [SerializeField] private Color _selectedbuttonColor;
        [SerializeField] private GameObject tutoPanel;
        [SerializeField] private GameObject optionsButton;
        [SerializeField] private float _bounceDuration  = 0.2f;
        [SerializeField] private GameObject _outlineImage;
        
        private static Button _currentButton;
        
        private Tween _bounceTween;

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
                //transform.DOScale(1.1f, _bounceDuration).SetUpdate(true).SetEase(Ease.OutBounce);
                
                _bounceTween.Kill();
                transform.localScale = Vector3.one;
                _bounceTween = transform.DOScale(1.05f, _bounceDuration).SetUpdate(true).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);;
                buttonText.color = _selectedTextColor;
                if (_outlineImage)
                {
                    _outlineImage.SetActive(true);
                }

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
                _bounceTween.Kill();
                transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InOutSine);
                buttonText.color = _deselectedTextColor;

                if (_outlineImage)
                {
                    _outlineImage.SetActive(false);
                }
                
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
                
                if (GetComponent<TMP_Dropdown>())
                {
                    optionsButton.transform.DOScale(1.0f, _bounceDuration).SetUpdate(true).SetEase(Ease.InOutSine);
                    buttonText.color = _deselectedbuttonColor;
                }
            }
        }
        

        public void OnSubmit(BaseEventData eventData)
        { 
            transform.localScale = Vector3.one;
            _bounceTween.Kill();
            transform.DOPunchScale(Vector3.one * 0.2f, _bounceDuration).SetUpdate(true).SetEase(Ease.Linear);
        }
    }
}