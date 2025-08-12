using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DotWeenEffect : MonoBehaviour
{
    [SerializeField] private float _fadeValue;
    [SerializeField] private float _duration;
    void Start()
    {
        //transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine); 
        GetComponent<CanvasGroup>().DOFade(_fadeValue, _duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
