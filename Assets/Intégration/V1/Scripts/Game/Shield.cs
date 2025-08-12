using System.Collections;
using UnityEngine;
using FlowerController = Intégration.V1.Scripts.Game.Characters.FlowerController;

namespace Intégration.V1.Scripts.Game
{
    public class Shield : MonoBehaviour
    {
        Renderer _renderer;
        private FlowerController _holderFlowerController;
        private Collider _collider;
        [SerializeField] AnimationCurve _DisplacementCurve;
        [SerializeField] float _DisplacementMagnitude;
        [SerializeField] float _LerpSpeed;
        [SerializeField] float _DisolveSpeed;
        [SerializeField] private float _autoCloseDelay = 2f;
        public bool _shieldOn;
        bool _isAnimating;
        Coroutine _disolveCoroutine;

        void Start()
        {
            _holderFlowerController = GetComponentInParent<FlowerController>();
            _renderer = GetComponent<Renderer>();
            _collider = GetComponent<Collider>();
            _renderer.material.SetFloat("_Disolve", 1);
            _isAnimating = false;
            _collider.enabled = false;
        }

        void Update()
        {
           /* if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main!.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    HitShield(hit.point);
                }
            }
            if (Input.GetKeyDown(KeyCode.F) && !_shieldOn && !_isAnimating)
            {
                OpenShield();
            }
            */
        }

        public void HitShield(Vector3 hitPos)
        {
            _renderer.material.SetVector("_HitPos", hitPos);
            StopAllCoroutines();
            StartCoroutine(Coroutine_HitDisplacement());
        }

        public void OpenShield()
        {
            if (!_isAnimating && !_shieldOn)
            {
                _holderFlowerController.isInvincible = true;
                _collider.enabled = true;
                _shieldOn = true;
                _isAnimating = true; 
                if (_disolveCoroutine != null)
                {
                    StopCoroutine(_disolveCoroutine);
                }
                _disolveCoroutine = StartCoroutine(Coroutine_DisolveAndAutoClose(0)); 
            }
        }

        IEnumerator Coroutine_HitDisplacement()
        {
            float lerp = 0;
            while (lerp < 1)
            {
                _renderer.material.SetFloat("_DisplacementStrength", _DisplacementCurve.Evaluate(lerp) * _DisplacementMagnitude);
                lerp += Time.deltaTime * _LerpSpeed;
                yield return null;
            }
        }

        IEnumerator Coroutine_DisolveAndAutoClose(float target)
        {
            float start = _renderer.material.GetFloat("_Disolve");
            float lerp = 0;
            while (lerp < 1)
            {
                _renderer.material.SetFloat("_Disolve", Mathf.Lerp(start, target, lerp));
                lerp += Time.deltaTime * _DisolveSpeed;
                yield return null;
            }
            _renderer.material.SetFloat("_Disolve", target);

            if (target == 0) 
            {
                yield return new WaitForSeconds(_autoCloseDelay);
                CloseShield(); 
            }
            else
            {
                _isAnimating = false; 
            }
        }

        private void CloseShield()
        {
            _collider.enabled = false;
            _shieldOn = false;
            _isAnimating = true;
            if (_disolveCoroutine != null)
            {
                StopCoroutine(_disolveCoroutine);
            }
            _disolveCoroutine = StartCoroutine(Coroutine_DisolveAndAutoClose(1)); 
            
            FlowerController[] allFlowers = FindObjectsOfType<FlowerController>();
            foreach (FlowerController flower in allFlowers)
            {
                flower.isInvincible = false;
            }
        }
        
    }
}
