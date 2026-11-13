using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.Game.FeedBack;
using Intégration.V1.Scripts.Menu;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V2_REFACTO.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    public static Action OnPlayerStateChanged;
    public static Action OnPlayersCanceledSelection;
    public bool PlayersSelectionConfirmed;

    [Header("Player manager")]
    PlayerInputManager _playerInputManager;
    [SerializeField] private Transform _playerUIParent;
    [SerializeField] private int _minPlayers;
    [SerializeField] private bool _allowAITurtle = true;
    private bool AllPlayersReady;
    private bool _aiTurtleMode;

    [Header("AI Flowers")]
    [SerializeField] private int _maxFlowers = 4;
    [SerializeField] private int _soloTurtleDefaultAIFlowers = 3;
    private int _aiFlowerCount;
    private bool _aiFlowerCountTouched;
    private readonly List<int> _aiFlowerChoices = new();
    private readonly Dictionary<int, GameObject> _aiCards = new();
    private TextMeshProUGUI _aiFlowersLabel;
    private string _aiFlowersLabelText = "AI";
    [SerializeField] private Sprite _aiFlowersRbImage;
    [SerializeField] private Sprite _aiFlowersLbImage;


    [Header("Start Game Ui References")]
    [SerializeField] private GameObject _startgamePopUp;
    [SerializeField] private GameObject _choseCharacterPopUp;
    [SerializeField] private Image holdFillImage;
    [SerializeField] private float holdTime = 1.5f;
    [SerializeField] private GameObject _choseMapPanel;
    private Tween fillTween;

    private bool _characterSelectionCanceled;
    private bool isStartingGame = false;

    private Dictionary<InputDevice, PlayerInput> deviceToPlayerInput = new();
    private List<PlayerInput> playerInputsList = new() ;
    private List<GameObject> playersUiList = new();
    private bool _turtleIsSelected;
    private int _playersReadyCount;
    private List<PlayerInput> holdingPlayers = new();

    [Header("HoldConfirm Actions")]
    private Dictionary<PlayerInput, InputAction> playerStartActions = new();
    private Dictionary<PlayerInput, Action<InputAction.CallbackContext>> startHandlers = new();
    private Dictionary<PlayerInput, Action<InputAction.CallbackContext>> cancelHandlers = new();

    [Header("Transition references")]
    [SerializeField] private GameObject circularTransition;

    private void Awake()
    {
        _playerInputManager = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        RemoveOrphanInputUsers();
        DataManager.Instance.AIFlowerChoices.Clear();

        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                CheckCurrentGamepads(device);
            }
        }

        InputSystem.onDeviceChange += OnDeviceChange;
        OnPlayerStateChanged += CheckPlayersReady;
        OnPlayersCanceledSelection += OnCancelSelection;
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        RefreshAIFlowersLabelText();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        OnPlayerStateChanged -= CheckPlayersReady;
        OnPlayersCanceledSelection -= OnCancelSelection;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    private void OnDestroy()
    {
        foreach (var player in playerInputsList)
        {
            UnRegisterPlayer(player);
            player.DeactivateInput();
        }
    }

    private void Update()
    {
        if (!AllPlayersReady || PlayersSelectionConfirmed || isStartingGame) return;

        int delta = 0;
        foreach (var player in playerInputsList)
        {
            if (player == null) continue;
            foreach (var device in player.devices)
            {
                if (device is not Gamepad gamepad) continue;
                if (gamepad.rightShoulder.wasPressedThisFrame) delta++;
                if (gamepad.leftShoulder.wasPressedThisFrame) delta--;
            }
        }

        if (delta == 0) return;

        int previous = _aiFlowerCount;
        _aiFlowerCountTouched = true;
        _aiFlowerCount += delta;
        CheckPlayersReady();
        if (_aiFlowerCount != previous)
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonNavigate);
    }

    private static void RemoveOrphanInputUsers()
    {
        foreach (var user in new List<InputUser>(InputUser.all))
        {
            bool owned = false;
            foreach (var playerInput in PlayerInput.all)
            {
                if (playerInput.user == user)
                {
                    owned = true;
                    break;
                }
            }
            if (!owned) user.UnpairDevicesAndRemoveUser();
        }
    }

    private void CheckPlayersReady()
    {
        _playersReadyCount = 0;
        _turtleIsSelected = false;

        foreach (var player in playerInputsList)
        {
            PlayerSelection playerSelection = player.GetComponent<PlayerSelection>();

            if (playerSelection.CurrentState == PlayerState.Joined)
            {
                _aiTurtleMode = false;
                HideAllAICards();
                _startgamePopUp.SetActive(false);
                _choseCharacterPopUp.SetActive(true);
                AllPlayersReady = false;
                UpdateHoldProgress();
                return;
            }
            else if (playerSelection.CurrentState == PlayerState.Ready)
            {
                _playersReadyCount++;
                if (playerSelection.TurtleIsSelected)
                {
                    _turtleIsSelected = true;
                }
            }
        }

        UpdateAIFlowerCount();

        bool canStartWithHumanTurtle = _turtleIsSelected && _playersReadyCount + _aiFlowerCount >= _minPlayers;
        bool canStartWithAITurtle = _allowAITurtle && !_turtleIsSelected && _playersReadyCount >= 1;
        _aiTurtleMode = canStartWithAITurtle && !canStartWithHumanTurtle;
        if (!canStartWithHumanTurtle && !canStartWithAITurtle)
        {
            HideAllAICards();
            return;
        }

        _choseCharacterPopUp.SetActive(false);
        _startgamePopUp.SetActive(true);
        AllPlayersReady = true;

        UpdateAIFlowerChoices();
        RefreshAICards();
        RefreshAIFlowersLabel();
    }

    private void UpdateAIFlowerCount()
    {
        int humanFlowers = _playersReadyCount - (_turtleIsSelected ? 1 : 0);
        int flowerTypes = Mathf.Max(0, CharacterCount() - 1);
        int max = Mathf.Clamp(Mathf.Min(_maxFlowers, flowerTypes) - humanFlowers, 0, _maxFlowers);
        bool soloTurtle = _turtleIsSelected && humanFlowers == 0;
        int min = soloTurtle ? Mathf.Min(1, max) : 0;

        if (!_aiFlowerCountTouched)
            _aiFlowerCount = soloTurtle ? _soloTurtleDefaultAIFlowers : 0;

        _aiFlowerCount = Mathf.Clamp(_aiFlowerCount, min, max);
    }

    private void UpdateAIFlowerChoices()
    {
        var taken = new HashSet<int>();
        foreach (var info in DataManager.Instance.PlayerChoice.Values)
            taken.Add(info.prefabIndex);

        _aiFlowerChoices.RemoveAll(taken.Contains);
        while (_aiFlowerChoices.Count > _aiFlowerCount)
            _aiFlowerChoices.RemoveAt(_aiFlowerChoices.Count - 1);

        int turtleIndex = TurtleIndex();
        var available = new List<int>();
        for (int i = 0; i < CharacterCount(); i++)
        {
            if (i != turtleIndex && !taken.Contains(i) && !_aiFlowerChoices.Contains(i))
                available.Add(i);
        }

        while (_aiFlowerChoices.Count < _aiFlowerCount && available.Count > 0)
        {
            int pick = Random.Range(0, available.Count);
            _aiFlowerChoices.Add(available[pick]);
            available.RemoveAt(pick);
        }
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad) return;
        if (change == InputDeviceChange.Added)
        {
            if (!PlayersSelectionConfirmed)
            {
                RumbleManager.Instance.RumblePulse(gamepad);
                CheckCurrentGamepads(gamepad);
            }
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            ReturnSelectionPanel();
        }
    }

    private void CheckCurrentGamepads(InputDevice device)
    {
        if (deviceToPlayerInput.ContainsKey(device) || _playerInputManager.playerCount >= _playerInputManager.maxPlayerCount) return;

        _playerInputManager.JoinPlayer(-1, -1, null, device);
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        GameObject playerUi = playerInput.transform.parent.gameObject;
        playersUiList.Add(playerUi);
        playerUi.transform.SetParent(_playerUIParent);

        if (deviceToPlayerInput.ContainsKey(device))
        {
            return;
        }

        deviceToPlayerInput[device] = playerInput;
        playerInputsList.Add(playerInput);
        RegisterPlayer(playerInput);
    }

    public void ChoiceMap(int mapindex)
    {
        if (mapindex == 1)
        {
            DataManager.Instance.CurrentMap = DataManager.MapChoice.Garden;
        }
        else if (mapindex == 2)
        {
            DataManager.Instance.CurrentMap = DataManager.MapChoice.Laboratory;
        }
        else
        {
            DataManager.Instance.CurrentMap = DataManager.MapChoice.Garden;
        }
    }

    public void LoadGameSene()
    {
        foreach (var player in playerInputsList)
        {
            UnRegisterPlayer(player);
        }
        if (!isStartingGame)
        {
            DataManager.Instance.AIFlowerChoices.Clear();
            DataManager.Instance.AIFlowerChoices.AddRange(_aiFlowerChoices);

            AudioManager.Instance.ChangeMusic(AudioManager.Instance.ClipsIndex.GameMusic);

            switch (DataManager.Instance.CurrentMap)
            {
                case DataManager.MapChoice.Garden:
                    CustomSceneManager.Instance.LoadScene("Game_Garden",true);
                    break;
                case DataManager.MapChoice.Laboratory:
                    CustomSceneManager.Instance.LoadScene("Game_Labo");
                    break;
            }

            isStartingGame = true;
        }
    }

    #region AI Cards

    private Button[] CharacterButtons()
    {
        foreach (var player in playerInputsList)
        {
            if (player == null) continue;
            var selection = player.GetComponent<PlayerSelection>();
            if (selection != null && selection.CharacterButtons != null && selection.CharacterButtons.Length > 0)
                return selection.CharacterButtons;
        }
        return Array.Empty<Button>();
    }

    private int CharacterCount() => CharacterButtons().Length;

    private int TurtleIndex()
    {
        Button[] buttons = CharacterButtons();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null && buttons[i].CompareTag("Turtle")) return i;
        }
        return buttons.Length - 1;
    }

    private Color CharacterColor(int characterIndex)
    {
        Button[] buttons = CharacterButtons();
        if (characterIndex >= 0 && characterIndex < buttons.Length && buttons[characterIndex] != null
            && buttons[characterIndex].TryGetComponent(out UISelectionFeedBack feedback))
            return feedback.OutlineColor;
        return Color.white;
    }

    private void RefreshAICards()
    {
        var wanted = new List<int>();
        if (_aiTurtleMode) wanted.Add(TurtleIndex());
        wanted.AddRange(_aiFlowerChoices);

        foreach (var kvp in _aiCards)
        {
            if (kvp.Value != null && !wanted.Contains(kvp.Key)) kvp.Value.SetActive(false);
        }

        foreach (int characterIndex in wanted)
        {
            GameObject card = GetOrCreateAICard(characterIndex);
            if (card == null || card.activeSelf) continue;

            card.SetActive(true);
            card.transform.SetAsLastSibling();
            card.transform.localScale = Vector3.one * 0.5f;
            card.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);

            if (characterIndex == TurtleIndex())
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleVoice);
            else
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
        }
    }

    private GameObject GetOrCreateAICard(int characterIndex)
    {
        if (_aiCards.TryGetValue(characterIndex, out GameObject existing) && existing != null) return existing;

        GameObject holder = new GameObject("AI Card Holder");
        holder.SetActive(false);

        GameObject card = Instantiate(_playerInputManager.playerPrefab, holder.transform);
        card.name = "AI Card " + characterIndex;

        PlayerSelection selection = card.GetComponentInChildren<PlayerSelection>(true);
        selection.ShowAsAICard(characterIndex, characterIndex == TurtleIndex(), CharacterColor(characterIndex));
        DestroyImmediate(selection.gameObject);

        card.SetActive(false);
        card.transform.SetParent(_playerUIParent, false);
        Destroy(holder);

        _aiCards[characterIndex] = card;
        return card;
    }

    private void HideAllAICards()
    {
        foreach (var card in _aiCards.Values)
        {
            if (card != null) card.SetActive(false);
        }
        if (_aiFlowersLabel != null) _aiFlowersLabel.gameObject.SetActive(false);
    }

    private void RefreshAIFlowersLabel()
    {
        if (_aiFlowersLabel == null) CreateAIFlowersLabel();
        if (_aiFlowersLabel == null) return;

        _aiFlowersLabel.gameObject.SetActive(true);
        _aiFlowersLabel.text = $"{_aiFlowersLabelText} : {_aiFlowerCount} : <sprite index=123> - <sprite index=124>";
    }

    private void CreateAIFlowersLabel()
    {
        TextMeshProUGUI source = _choseCharacterPopUp.GetComponentInChildren<TextMeshProUGUI>(true);
        if (source == null) return;

        GameObject clone = Instantiate(source.gameObject, _startgamePopUp.transform, false);
        clone.name = "AI Flowers Label";
        foreach (var component in clone.GetComponents<MonoBehaviour>())
        {
            string ns = component.GetType().Namespace;
            if (ns != null && ns.StartsWith("UnityEngine.Localization")) DestroyImmediate(component);
        }

        var rect = (RectTransform)clone.transform;
        rect.anchorMin = new Vector2(1f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.pivot = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(440f, 60f);
        rect.anchoredPosition = new Vector2(-30f, 0f);

        _aiFlowersLabel = clone.GetComponent<TextMeshProUGUI>();
        _aiFlowersLabel.alignment = TextAlignmentOptions.MidlineRight;
        _aiFlowersLabel.enableAutoSizing = true;
        _aiFlowersLabel.fontSizeMin = 14f;
        _aiFlowersLabel.fontSizeMax = Mathf.Max(20f, source.fontSize * 0.6f);
    }

    private void OnLocaleChanged(Locale locale) => RefreshAIFlowersLabelText();

    private void RefreshAIFlowersLabelText()
    {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Translate", "AI Flowers Label");
        handle.Completed += operation =>
        {
            if (this == null || string.IsNullOrEmpty(operation.Result)) return;
            _aiFlowersLabelText = operation.Result;
            if (AllPlayersReady && _aiFlowersLabel != null) RefreshAIFlowersLabel();
        };
    }

    #endregion

    #region Input Actions
    private void RegisterPlayer(PlayerInput player)
    {
        var action = player.actions["StartGame"];
        Action<InputAction.CallbackContext> startHandler = ctx => OnStartGame(player, ctx);
        Action<InputAction.CallbackContext> cancelHandler = ctx => OnCancelGame(player, ctx);

        action.performed += startHandler;
        action.canceled += cancelHandler;

        playerStartActions[player] = action;
        startHandlers[player] = startHandler;
        cancelHandlers[player] = cancelHandler;
    }

    private void UnRegisterPlayer(PlayerInput player)
    {
        if (playerStartActions.TryGetValue(player, out var action))
        {
            if (startHandlers.TryGetValue(player, out var start))
            {
                action.performed -= start;
                startHandlers.Remove(player);
            }
            if (cancelHandlers.TryGetValue(player, out var cancel))
            {
                action.canceled -= cancel;
                cancelHandlers.Remove(player);
            }

            playerStartActions.Remove(player);
        }
    }

    private void OnStartGame(PlayerInput player, InputAction.CallbackContext context)
    {
        if (!AllPlayersReady) return;
        if (PlayersSelectionConfirmed) return;

       AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonPressed);
        holdingPlayers.Add(player);
        UpdateHoldProgress();
    }

    private void OnCancelGame(PlayerInput player, InputAction.CallbackContext context)
    {
        holdingPlayers.Remove(player);
        UpdateHoldProgress();
    }

    private void OnCancelSelection()
    {
        if (PlayersSelectionConfirmed)
        {
            ReturnSelectionPanel();
        }
        else
        {
            foreach (var playerInput in playerInputsList)
            {
                PlayerSelection playerSelection = playerInput.GetComponent<PlayerSelection>();

                if (playerSelection.CurrentState != PlayerState.NotJoined)
                {
                    return;
                }
            }

            PlayersSelectionConfirmed = false;

            if (!_characterSelectionCanceled)
            {
                _characterSelectionCanceled = true;
                CustomSceneManager.Instance.LoadScene("Refacto Menu");
            }
        }
    }

    private void ReturnSelectionPanel()
    {
        PlayersSelectionConfirmed = false;
        _choseMapPanel.SetActive(false);
        OnPlayerStateChanged.Invoke();
    }

    private void UpdateHoldProgress()
    {
        if (holdingPlayers.Count > 0 && AllPlayersReady)
        {
            if (fillTween != null && fillTween.IsActive()) return;

            holdFillImage.transform.parent.DOScale(1.1f, 0.5f);
            fillTween = holdFillImage.DOFillAmount(1f, holdTime)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIButtonPressed);
                    _choseMapPanel.SetActive(true);
                    PlayersSelectionConfirmed = true;
                });
        }
        else
        {
            holdFillImage.transform.parent.DOScale(1f, 0.5f);
            holdFillImage.DOFillAmount(0f, 1f).SetEase(Ease.OutQuad);
            holdingPlayers?.Clear();
            fillTween?.Kill();
        }
    }

    #endregion
}
