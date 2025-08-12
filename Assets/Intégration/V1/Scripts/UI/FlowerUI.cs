using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.Game.Characters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.UI
{
    public class FlowerUI : MonoBehaviour
    {
        public FlowerController FlowerPlayer;
        [SerializeField] private List<Sprite> playerCharacterImages;
        [SerializeField] private List<Sprite> playerDeadCharacterImages;
        [SerializeField] private List<Sprite> CapacityIcons;
        [SerializeField] private List<Image> sunImage;
        [SerializeField] private TextMeshProUGUI _playerIndexText;
        [SerializeField] private Sprite fullSun;
        [SerializeField] private Sprite emptySun;
        [SerializeField] private Image characterIcon;
        [SerializeField] private Image capacityIcon;


        private void OnEnable()
        {
            if (FlowerPlayer != null)
            {
                FlowerPlayer.OnSunChanged += HandleSunChange;
                FlowerPlayer.OnDeathChanged += HandleDeathChange;
            }
        }

        private void OnDisable()
        {
            if (FlowerPlayer != null)
            {
                FlowerPlayer.OnSunChanged -= HandleSunChange;
                FlowerPlayer.OnDeathChanged -= HandleDeathChange;
            }
        }
        
        
        private void Start()
        {
            if (FlowerPlayer == null) return;
            
            capacityIcon.sprite = CapacityIcons[FlowerPlayer.characterIndex];
            _playerIndexText.text = "J" + (FlowerPlayer.PlayerIndex+1);
            
            HandleSunChange(FlowerPlayer.Sun);
            HandleDeathChange(FlowerPlayer.isDead);

        }

       
        
        private void HandleSunChange(int sun)
        {
            // Update sun visuals
            for (int i = 0; i < sunImage.Count; i++)
            {
                if (i < sun)
                {
                    sunImage[i].sprite = fullSun;
                    sunImage[i].transform.DOScale(1.1f, 0.3f);
                }
                else
                {
                    sunImage[i].sprite = emptySun;
                    sunImage[i].transform.DOScale(1f, 0.3f);
                }
            }

            // Update capacity icon appearance
            if (sun >= FlowerPlayer.CapacityCost)
            {
                capacityIcon.color = Color.white;
                capacityIcon.rectTransform.DOKill();
                capacityIcon.rectTransform.DOScale(1.1f, 0.5f).SetEase(Ease.OutSine).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                capacityIcon.color = Color.grey;
                capacityIcon.rectTransform.DOKill(); // kill the loop if it's active
                capacityIcon.rectTransform.DOScale(1f, 0.2f);
            }
        }
        
        private void HandleDeathChange(bool isDead)
        {
            int index = FlowerPlayer.characterIndex;
            characterIcon.sprite = isDead ? playerDeadCharacterImages[index] : playerCharacterImages[index];
        }
        
        
        
    }
}