using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.Localization.Settings;

public class GameIntro : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private CanvasGroup _introPanelCG;

    [SerializeField] private RectTransform _scientistImage;
    [SerializeField] private RectTransform _turtleImage;
    [SerializeField] private RectTransform _turtleDialogueBox;
    [SerializeField] private RectTransform _scientistDialogueBox;
    [SerializeField] private TextMeshProUGUI _turtleSubtitleText;
    [SerializeField] private TextMeshProUGUI _scientistSubtitleText;
    [SerializeField] private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject _skipButtons;
    [Header("Settings")] [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float typeDelay = 0.1f;
    [SerializeField] private float pauseBetween = 0.3f;

    private Sequence _dialogueSeq;

    public static bool IsPlaying { get; private set; }

    void Start()
    {
        IsPlaying = true;
        _scientistSubtitleText.text = "";
        _turtleSubtitleText.text = "";

        Invoke(nameof(GameIntroDialogue), 1.5f);
    }

    private void GameIntroDialogue()
    {
        eventSystem.SetSelectedGameObject(_skipButtons);
        Time.timeScale = 0;

        _dialogueSeq = DOTween.Sequence();
        _dialogueSeq.SetUpdate(true);

        _dialogueSeq.Append(_introPanelCG.DOFade(1, 0.5f));

        _dialogueSeq.Append(_scientistImage.DOAnchorPosX(350, slideDuration).SetEase(Ease.OutBack));
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIPopPanel));
        _dialogueSeq.Append(_scientistDialogueBox.DOScale(1, 0.3f).SetEase(Ease.OutBack));

        if (LocalizationSettings.SelectedLocale.Identifier.Code == "fr")
        {
            _dialogueSeq.AppendCallback(() => TypeText(_scientistSubtitleText, "Je compte sur toi, turbo tortue !"));
            _dialogueSeq.AppendInterval("Je compte sur toi, turbo tortue !".Length * typeDelay + pauseBetween);
        }
        else if ((LocalizationSettings.SelectedLocale.Identifier.Code == "en"))
        {
            _dialogueSeq.AppendCallback(() => TypeText(_scientistSubtitleText, "I'm counting on you, turbo tortue!"));
            _dialogueSeq.AppendInterval("I'm counting on you, turbo tortue!".Length * typeDelay + pauseBetween);
        }

        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.ScientistDialogue));
        _dialogueSeq.Append(_scientistDialogueBox.DOScale(0, 0.3f));

        _dialogueSeq.AppendInterval(pauseBetween);

        _dialogueSeq.Append(_turtleImage.DOAnchorPosX(-350, slideDuration).SetEase(Ease.OutBack));
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIPopPanel));
        _dialogueSeq.Append(_turtleDialogueBox.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        _dialogueSeq.AppendCallback(() => TypeText(_turtleSubtitleText, "pip.pap*&^!@!#*~…bop"));
        _dialogueSeq.AppendInterval("pip.pap*&^!@!#*~…bop".Length * typeDelay + pauseBetween);
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleDialogue));
        _dialogueSeq.Append(_turtleDialogueBox.DOScale(0, 0.3f));

        _dialogueSeq.AppendInterval(pauseBetween+0.2f);

        _dialogueSeq.Append(_introPanelCG.DOFade(0,1f));
        _dialogueSeq.Join(_scientistImage.DOAnchorPosX(0, slideDuration).SetEase(Ease.InBack));
        _dialogueSeq.Join(_turtleImage   .DOAnchorPosX(0,  slideDuration).SetEase(Ease.InBack));

        _dialogueSeq.OnComplete(SkipIntro);
    }

    public void SkipIntro()
    {
        IsPlaying = false;
        eventSystem.SetSelectedGameObject(null);
        _dialogueSeq.Kill();
        _introPanelCG.DOFade(0,1f).SetUpdate(true);
        Time.timeScale = 1;
    }

    private void OnDestroy()
    {
        IsPlaying = false;
    }

    private void TypeText(TextMeshProUGUI label, string message)
    {
        label.text = "";
        int total = message.Length;
        int curr = 0;

        DOTween.To(() => curr, x => {
                curr = x;
                label.text = message.Substring(0, curr);
            }, total, total * typeDelay)
            .SetEase(Ease.Linear)
            .SetUpdate(true);
    }
}
