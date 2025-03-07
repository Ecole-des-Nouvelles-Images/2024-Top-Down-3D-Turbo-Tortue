using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerSelection : MonoBehaviour
{
    [Header("Player UI References")]
    [SerializeField] private GameObject _playerUi;
    [SerializeField] private Image _playerIndexImage;
    [SerializeField] private Image _characterIcon;
    [SerializeField] private TextMeshProUGUI _playerIndexText;
    [SerializeField] private GameObject _joinVisuals;
    [SerializeField] private GameObject _flowerVisuals;
    [SerializeField] private Image _capacityImage;
    [SerializeField] private GameObject _selectorUi;
    [SerializeField] private List<Sprite> _characterSprites;
    [SerializeField] private List<Sprite> _capacitiesSprites;
    
    [Header("Grid UI References")]
    [SerializeField] private List<Button> _characterButtons;
    private Button _selectedCharacterButton;
    
    private PlayerInput _playerInput;
    private int _userIndex;
    private RectTransform _globalPanel;
    private readonly Vector3 _playerUiScale = new Vector3(0.5f, 0.5f, 0.5f);
    private Gamepad _gamepad;
    private EventSystem _eventSystem;

    private bool _isReady; 
    private bool _isJoining;
    
    
    private void Awake()
    {
        _globalPanel = GameObject.Find("GlobalPanel").GetComponent<RectTransform>();
        _playerUi = transform.parent.gameObject;
        _playerInput = transform.parent.GetComponentInChildren<PlayerInput>();
        _userIndex = _playerInput.user.index;
        _playerIndexText.text = (_userIndex + 1).ToString() ;
        _gamepad = _playerInput.devices[0] as Gamepad;
        _eventSystem = GetComponentInChildren<EventSystem>();
    }

    
    private void Start()
    {
        _playerInput.gameObject.transform.SetParent(_playerInput.transform.parent.parent);
        GamepadConnectionFeedback();
    }

    private void Update()
    {
    }

    private void GamepadConnectionFeedback()
    {
        _playerUi.transform.localScale = _playerUiScale;
        _playerUi.transform.DOScale(Vector3.one, 0.1f);
        CameraShakeEffect();
    }

    private void GamepadResetFeedBack()
    { 
        _playerUi.transform.localScale = _playerUiScale;
    }
    
    public void OnNavigate()
    {
        RadialGridLayoutGroup radialLayoutGroup = _eventSystem.currentSelectedGameObject.GetComponentInChildren<RadialGridLayoutGroup>();
        _selectorUi.transform.SetParent(radialLayoutGroup.transform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(radialLayoutGroup.transform as RectTransform);
        _selectorUi.transform.localScale = Vector3.one;
        radialLayoutGroup.enabled = false;
        radialLayoutGroup.enabled = true;
        
        Debug.Log("OnNavigate");
        if (!_isJoining)
        {
            UiBounceEffect(_playerUi,1.05f,0.2f);
        }
        
    }

    public void OnSubmit()
    {
        if (!_isJoining)
        {
            PlayerJoined();
        }
        else if (!_isReady)
        {
            PlayerReady();
        }
       
    }

    public void OnCancel()
    {
        Debug.Log("CANCEL");
        
        if (_isReady)
        {
            //Cancel ready 
           _isReady = false;
        }
        else if (_isJoining)
        {
            //Cancel Join 
            _isJoining = false;
            _flowerVisuals.SetActive(false);
            _joinVisuals.SetActive(true);
        }
    }
    

    private void PlayerJoined()
    {
        Debug.Log("JOINING");
        _isJoining = true;
        _flowerVisuals.SetActive(true);
        _joinVisuals.SetActive(false);
        UiBounceEffect(_playerUi,1.1f,0.3f);
    }

    private void PlayerReady()
    {
        Debug.Log("Ready");
        _isReady = true;
    }
    
    public void OnDeviceLost()
    {
        Debug.Log("OnDeviceLost");
        OnCancel();
        _playerUi.SetActive(false); 
    }

    public void OnDeviceRegained()
    {
        Debug.Log("OndeviceRegained");
        GamepadConnectionFeedback();
        _playerUi.SetActive(true); 
    }

    private void CameraShakeEffect()
    { 
        _globalPanel.DOShakePosition(0.2f, 5f, 10, 5,false, true);
    }

    private void UiBounceEffect(GameObject ui, float scaleBounce, float duration)
    {
        ui.transform.DOScale(scaleBounce, duration/2).OnComplete(() =>
        {
            ui.transform.DOScale(1f, duration/2);
        });
    }
   
    
   
}