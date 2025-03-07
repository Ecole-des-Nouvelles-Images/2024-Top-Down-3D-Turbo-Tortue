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
       [SerializeField] private GameObject _outlineSelector;
       [SerializeField] private AudioSource _selectedSound;
       [SerializeField] private Color _outlineColor;

       private void Start()
       {
           _outlineSelector.GetComponent<Image>().color = _outlineColor;
       }

       public void OnSelect(BaseEventData eventData)
        {
            _outlineSelector.SetActive(true);
            transform.DOScale(1.1f, 0.5f);
            _selectedSound.Play();
        }

        public void OnDeselect(BaseEventData eventData)
        {
           /* if (GetComponentInChildren<HorizontalLayoutGroup>().transform.childCount <= 1)
            {
                _outlineSelector.SetActive(false);
                transform.DOScale(1.0f, 0.5f);
            }*/
           _outlineSelector.SetActive(false);
           transform.DOScale(1.0f, 0.5f);
        }
    }
}