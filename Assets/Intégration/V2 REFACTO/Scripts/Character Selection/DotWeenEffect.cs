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
        GetComponent<CanvasGroup>().DOFade(_fadeValue, _duration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }

    void Update()
    {
    }
}
