using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UIElements.Image;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _characterIcon;
    [SerializeField] private TextMeshProUGUI _playerNumberText;
    [SerializeField] private TextMeshProUGUI _stat01Text;
    [SerializeField] private TextMeshProUGUI _stat02Text;
    [SerializeField] private TextMeshProUGUI _stat03Text;
    [SerializeField] private TextMeshProUGUI _statTitleText;
    
    [SerializeField] private List<Sprite> playerCharacterSprites;





    public void Setup(PlayerStats stats)
    {
        _playerNumberText.text = "J" + stats.playerIndex;
        _characterIcon.sprite = playerCharacterSprites[stats.characterIndex];
        
        if (stats.IsTurtle)
        {
            _stat01Text.text = "fleurs éliminées : " + stats.flowersEliminated;
            _stat02Text.text = "pièges placés : " + stats.trapsPlaced;
            _stat03Text.text = "soleil récupérés : " + stats.sunsCollected;
        }
        else
        {
            _stat01Text.text = "fleurs réanimées : " + stats.flowersRevived;
            _stat02Text.text = "temps caché : " + stats.timeSpentHidden;
            _stat03Text.text = "soleil récupérés : " + stats.sunsCollected;
        }
       
       
        
       
    }
}
