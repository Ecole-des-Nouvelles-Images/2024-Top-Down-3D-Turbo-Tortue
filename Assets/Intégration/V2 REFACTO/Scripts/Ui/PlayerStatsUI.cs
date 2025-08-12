using System.Collections.Generic;
using DG.Tweening;
using Michael.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI Elements")]
    
    [SerializeField] private Image _characterIcon;
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private TextMeshProUGUI _stat01Text;
    [SerializeField] private TextMeshProUGUI _stat02Text;
    [SerializeField] private TextMeshProUGUI _stat03Text;
    [SerializeField] private Image _statTitleImage;
    [SerializeField] private TextMeshProUGUI _statTitleText;
    
    [SerializeField] private List<Sprite> _FlowersSprites;
    [SerializeField] private List<Sprite> _DeadFlowerSprites;
    [SerializeField] private Sprite _turtleSprite;
    [SerializeField] private Sprite _deadTurtleSprite;



    public void Setup(PlayerStats stats)
    {
        _playerNumberText.text = "J" + (stats.playerIndex+1);
        _statTitleText.text = stats.GetTrophyTitle();
        _statTitleImage.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
        
        
        if (stats.IsTurtle)
        {
            _characterIcon.sprite = GameManager.Instance.FlowersIsDead ? _turtleSprite : _deadTurtleSprite;
           
            _stat01Text.text = "fleurs detectees : " + stats.flowersScanned;
            _stat02Text.text = "pieges places : " + stats.trapsPlaced;
            _stat03Text.text = "soleil recuperes : " + stats.sunsCollected;
        }
        else
        {
            _characterIcon.sprite = GameManager.Instance.FlowersIsDead ? _DeadFlowerSprites[stats.characterIndex] : _FlowersSprites[stats.characterIndex];
         
            _stat01Text.text = "fleurs reanimees : " + stats.flowersRevived;
            _stat02Text.text = "temps cache : " + Mathf.RoundToInt( stats.timeSpentHidden)+ "s";
            _stat03Text.text = "soleil recuperes : " + stats.sunsCollected;
        }
    }

}
