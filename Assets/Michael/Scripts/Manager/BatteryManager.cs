
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Michael.Scripts.Manager
{
    public class BatteryManager : MonoBehaviour
    {
        public static Action<float> OnBatteryDecrease;
        public static Action<float> OnSunCollected;
        public static bool NitroActivate;
        private float _currentBatteryTime;
        public float CurrentBatteryTime
        {
            get => _currentBatteryTime;
            set => _currentBatteryTime = Mathf.Clamp(value, 0f, _maxBatteryTime);
        }
        
        [SerializeField] private float _maxBatteryTime; 
        public float MaxBatteryTime => _maxBatteryTime;
        
        [SerializeField] private GameManager _gameManager;
      
       

        private void OnEnable()
        {
            OnBatteryDecrease += LoseBattery;
            OnSunCollected += SunCollected;
        }
        
        private void OnDisable()
        {
            OnBatteryDecrease -= LoseBattery;
            OnSunCollected -= SunCollected;
        }

        void Start()
        {
            CurrentBatteryTime = _maxBatteryTime;
        }
        
        void Update()
        {
            if (_gameManager.GameFinished) return;
            
            //Batterie qui baisse
            CurrentBatteryTime -= Time.deltaTime;
            if (NitroActivate)
            {
                CurrentBatteryTime -= Time.deltaTime * 13;
            }

            // Vérification de la mort de la tortue
            if (CurrentBatteryTime <= 0 && !GameManager.Instance.TurtleIsDead){
               
                GameManager.Instance.TurtleIsDead = true;
                GameManager.Instance.WinVerification();
            }
        }

        private void LoseBattery(float capacityCost)
        {
            if (_gameManager.GameFinished) return;
            
            CurrentBatteryTime -= capacityCost;
            GameManager.Instance.WinVerification();
        }

        private void SunCollected(float sunValue)
        {
            if (_gameManager.GameFinished) return;
            CurrentBatteryTime += sunValue;
        }
        
    }
}
