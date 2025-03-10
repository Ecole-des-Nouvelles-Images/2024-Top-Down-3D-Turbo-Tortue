using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Intégration.V1.Scripts.Game.FeedBack
{
    public class UISelectionFeedBack : MonoBehaviour, ISelectHandler, IDeselectHandler
    { 
        public Color OutlineColor;
        [SerializeField] private GameObject _outlineSelector;
        [SerializeField] private AudioSource _selectedSound;
       

       private void Start()
       {
           _outlineSelector.GetComponent<Image>().color = OutlineColor;
       }

       public void OnSelect(BaseEventData eventData)
        {
            _outlineSelector.SetActive(true);
            transform.DOScale(1.1f, 0.5f);
            SoundManager.PlaySound(SoundType.Selected,0.3f);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (GetComponentInChildren<RadialGridLayoutGroup?>().transform.childCount > 1) return;
            _outlineSelector.SetActive(false);
            transform.DOScale(1.0f, 0.5f);
        }
    }
}