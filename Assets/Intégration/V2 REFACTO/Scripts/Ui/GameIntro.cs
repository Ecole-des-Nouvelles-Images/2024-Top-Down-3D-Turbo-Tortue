using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;

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

    [Header("Settings")] [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float typeDelay = 0.1f;
    [SerializeField] private float pauseBetween = 0.3f;


    private Sequence _dialogueSeq;

    void Start()
    {
        _scientistSubtitleText.text = "";
        _turtleSubtitleText.text = "";
        Invoke(nameof(GameIntroDialogue), 1.5f);
    }

    private void GameIntroDialogue()
    {
        Time.timeScale = 0;

        _dialogueSeq = DOTween.Sequence();
        _dialogueSeq.SetUpdate(true);

        // Fade in panel
        _dialogueSeq.Append(_introPanelCG.DOFade(1, 0.5f));

        // dialogue du scientifique
        _dialogueSeq.Append(_scientistImage.DOAnchorPosX(350, slideDuration).SetEase(Ease.OutBack));
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIPopPanel));
        _dialogueSeq.Append(_scientistDialogueBox.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        _dialogueSeq.AppendCallback(() => TypeText(_scientistSubtitleText, "Je compte sur toi, turbo tortue"));
        _dialogueSeq.AppendInterval("Je compte sur toi, turbo tortue".Length * typeDelay + pauseBetween);
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.ScientistDialogue));
        _dialogueSeq.Append(_scientistDialogueBox.DOScale(0, 0.3f));
        
        // pause entre les 2 dialogues
        _dialogueSeq.AppendInterval(pauseBetween);
        
        // dialogue de la tortue
        _dialogueSeq.Append(_turtleImage.DOAnchorPosX(-350, slideDuration).SetEase(Ease.OutBack));
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.UIPopPanel));
        _dialogueSeq.Append(_turtleDialogueBox.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        _dialogueSeq.AppendCallback(() => TypeText(_turtleSubtitleText, "pip.pap*&^!@!#*~…bop"));
        _dialogueSeq.AppendInterval("pip.pap*&^!@!#*~…bop".Length * typeDelay + pauseBetween);
        _dialogueSeq.JoinCallback(() => AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleDialogue));
        _dialogueSeq.Append(_turtleDialogueBox.DOScale(0, 0.3f));
        
        _dialogueSeq.AppendInterval(pauseBetween+0.2f);
        
        //disparition des uis
        _dialogueSeq.Append(_introPanelCG.DOFade(0,1f));
        _dialogueSeq.Join(_scientistImage.DOAnchorPosX(0, slideDuration).SetEase(Ease.InBack));
        _dialogueSeq.Join(_turtleImage   .DOAnchorPosX(0,  slideDuration).SetEase(Ease.InBack));
        
        _dialogueSeq.OnComplete(() =>
        {
            Time.timeScale = 1;
        });
        
        
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
            .SetUpdate(true); // ← Ajout essentiel
    }
    
    
    
}
