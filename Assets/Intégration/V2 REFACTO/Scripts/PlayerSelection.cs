using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using Intégration.V1.Scripts.Game.FeedBack;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSelection : MonoBehaviour
{
    public event Action<GameObject> OnCursorMoved;
    public Action<RadialGridLayoutGroup, GameObject> OnPlayerReady;
    
    [Header("Player UI References")]
    [SerializeField] private GameObject _playerUi;
    [SerializeField] private Image _playerIndexImage;
    [SerializeField] private GameObject _joinVisuals;
    [SerializeField] private GameObject _flowerVisuals;
    [SerializeField] private Image _characterIcon;
    [SerializeField] private Image _capacityImage;
    [SerializeField] private GameObject _CursorUi;
    [SerializeField] private GameObject _readyPopup;
    [SerializeField] private GameObject _readyConfirmPopUp;
    [SerializeField] private List<Sprite> _characterSprites;
    [SerializeField] private List<Sprite> _capacitiesSprites;
    
    [Header("Grid UI References")]
    public Button[] CharacterButtons;
    
    private Button _selectedCharacterButton;
    private TextMeshProUGUI _playerIndexText;
    private PlayerInput _playerInput;
    private int _userIndex;
    private RectTransform _globalPanel;
    private EventSystem _eventSystem;
    private GameObject _lastGameObjectSelected;
    private Button _characterSelected;
    private bool _isReady; 
    private bool _isJoining;
    private bool _canChooseMap;
    private bool _TurtleIsSelected;
    
    private void Awake()
    {
        _globalPanel = GameObject.Find("GlobalPanel").GetComponent<RectTransform>();
        _playerUi = transform.parent.gameObject;
        _playerInput = transform.parent.GetComponentInChildren<PlayerInput>();
        _userIndex = _playerInput.user.index;
        _playerIndexText = _playerIndexImage.GetComponentInChildren<TextMeshProUGUI>();
        _playerIndexText.text = "J" +(_userIndex + 1).ToString() ;
        _CursorUi.GetComponentInChildren<TextMeshProUGUI>().text = "J" +(_userIndex + 1).ToString() ;
        _eventSystem = GetComponentInChildren<EventSystem>();
    }

    private void Start()
    {
        _playerInput.gameObject.transform.SetParent(_playerInput.transform.parent.parent);
        CharacterButtons = transform.parent.parent.parent.GetComponentsInChildren<Button>();
        GamepadConnectionFeedback();
    }
    
    private void UpdateCursor()
    {
        _characterSelected = _eventSystem.currentSelectedGameObject?.GetComponent<Button>();
        _playerIndexImage.color = _characterSelected.GetComponent<UISelectionFeedBack>().OutlineColor;
        
        if (_characterSelected == _lastGameObjectSelected) return;
        _lastGameObjectSelected = _characterSelected.gameObject;
        OnCursorMoved.Invoke(_characterSelected.gameObject);
        UpdateIconCharacter();
    }


    public void UpdateIconCharacter()
    {
        _characterSelected = _eventSystem.currentSelectedGameObject?.GetComponent<Button>();
        _characterIcon.sprite = _characterSprites[Array.IndexOf(CharacterButtons, _characterSelected)];
        switch (_characterSelected.tag)
        {
            case "Turtle":
                _capacityImage.gameObject.SetActive(false);
                break;
            case "Flower":
                _capacityImage.gameObject.SetActive(true);
                _capacityImage.sprite = _capacitiesSprites[Array.IndexOf(CharacterButtons, _characterSelected)];
                break;
        }
    }
    
    private void PlayerJoined()
    {
        Debug.Log("JOINING");
        _isJoining = true;
        _flowerVisuals.SetActive(true);
        _joinVisuals.SetActive(false);
        _CursorUi.SetActive(true);
        _playerIndexImage.gameObject.SetActive(true);
        _readyPopup.SetActive(true);
        UpdateCursor();
    }

    private void PlayerReady()
    {
        _characterSelected = _eventSystem.currentSelectedGameObject?.GetComponent<Button>();
        Debug.Log("Ready");
        _isReady = true;
        _characterSelected.GetComponent<CanvasGroup?>().alpha = 0.5f;
        _readyPopup.SetActive(false);
        _readyConfirmPopUp.SetActive(true);
        _characterSelected.enabled = false;
        CheckOtherCursor();
       
        switch (_characterSelected.tag)
        {
            case "Turtle":
                SoundManager.PlaySound(SoundType.TurtleMoveStart,0.3f); 
                _characterIcon.sprite = _characterSprites[^1];
                _TurtleIsSelected = true;
                break;
            case "Flower":
                SoundManager.PlaySound(SoundType.FlowerVoices,0.3f);
                break;
        }
        
        // player choices
    }

    private void CheckOtherCursor()
    {
        RadialGridLayoutGroup radialLayout = _characterSelected.GetComponentInChildren<RadialGridLayoutGroup>();
        if (radialLayout.transform.childCount < 2) return; 
        OnPlayerReady.Invoke(radialLayout, _CursorUi);
    }

    #region GamePad Input Actions

    public void OnNavigate()
    {
        if (_canChooseMap) return;
        
        if (!_isJoining)
        {
            UiBounceEffect(_playerUi,1.05f,1,0.2f);
            Debug.Log("navigate");
        }
        else if (!_isReady)
        {
            Invoke(nameof(UpdateCursor), 0f);
        }

    }
    
    public void OnSubmit()
    {
        if (_canChooseMap)
        {
            
        }
        else if (!_isJoining)
        {
            PlayerJoined();
            SoundManager.PlaySound(SoundType.Pressed,0.3f);
            UiBounceEffect(_playerUi,1.1f,_playerUi.transform.localScale.x,0.5f);
        }
        else if (!_isReady)
        {
            PlayerReady();
            CameraShakeEffect();
            RumbleManager.Instance.RumblePulse(1f, 1f, 0.1f, _playerInput?.devices[0] as Gamepad);
            SoundManager.PlaySound(SoundType.Pressed,0.3f);
            UiBounceEffect(_playerUi,1.2f,_playerUi.transform.localScale.x,0.5f);
            UiRotateEffect(_playerIndexImage.gameObject,0.5f);
        }
       
    }

    public void OnCancel()
    {
        if (_isReady)
        {
            SoundManager.PlaySound(SoundType.Canceled,0.3f);
            Debug.Log("CANCEL");
            //Cancel ready 
            _isReady = false;
            _TurtleIsSelected = false;
            _characterSelected.enabled = true;
            _characterSelected.GetComponent<CanvasGroup?>().alpha = 1f;
            _readyPopup.SetActive(true);
            _readyConfirmPopUp.SetActive(false);
            UpdateIconCharacter();

            //cancel player choice
        }
        else if (_isJoining)
        {
            SoundManager.PlaySound(SoundType.Canceled,0.3f);
            Debug.Log("CANCEL");
            //Cancel Join 
            _isJoining = false;
            _flowerVisuals.SetActive(false);
            _joinVisuals.SetActive(true);
            _CursorUi.transform.SetParent(_playerInput?.transform);
            _CursorUi.SetActive(false);
            _eventSystem.SetSelectedGameObject(null);
            _playerIndexImage.gameObject.SetActive(false);
            _readyPopup.SetActive(false);
        }
    }
    
    public void OnDeviceLost()
    {
        Debug.Log("OnDeviceLost");
        OnCancel();
        OnCancel();
        _playerUi.SetActive(false); 
    }

    public void OnDeviceRegained()
    {
        Debug.Log("OndeviceRegained");
        GamepadConnectionFeedback();
        _playerUi.SetActive(true); 
    }

    #endregion
    

    #region FeedBack Effects

    private void CameraShakeEffect()
    { 
        _globalPanel.DOShakePosition(0.2f, 5f, 10, 0,true, true)
            .OnComplete(() =>
            {
                _globalPanel.anchoredPosition = Vector2.zero;
                _globalPanel.sizeDelta = Vector2.zero;
               
            });
       
    }

    private void UiBounceEffect(GameObject ui, float scaleBounce,float initialScale, float duration)
    {
        ui.transform.DOScale(scaleBounce, duration/2).OnComplete(() =>
        {
            ui.transform.DOScale(1f, duration/2);
        });
    }
    
    private void UiRotateEffect(GameObject ui, float duration)
    {
        ui.transform.DORotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuad); // Effet fluide
    }
    
    private void GamepadConnectionFeedback()
    {
        _playerUi.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);;
        _playerUi.transform.DOScale(Vector3.one, 0.1f);
        CameraShakeEffect();
    }

    #endregion
   
  
  
  
    
   
}