using System.Collections;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Intégration.V1.Scripts.Game.Characters
{
    public class RoseController : FlowerController
    {
        [SerializeField] private GameObject spawnTrap;
        [SerializeField] private float respawnDelay = 5f;
        [SerializeField] private bool _isRespawning;
        private GameObject _currentTrap;
        

        protected override void PassiveCapacity()
        {
            isUnstoppable = true;
        }

        protected override void Start()
        {
            base.Start();
            PassiveCapacity();
        }
        

        protected override void Update()
        {
            base.Update();
            if (_currentTrap)
            {
                if (!isDead || _isRespawning) return;
                
                CanRespawn = true;
                _isRespawning = true;
                Invoke(nameof(Respawn),respawnDelay);
            }
            else
            {
                if (_isRespawning)
                {
                    GameManager.Instance.FlowersAlive.Remove(gameObject);
                    GameManager.Instance.WinVerification();
                    
                }
                CanRespawn = false;
                _isRespawning = false;
            }
        }

        protected override void MainCapacity() {
                if (Sun >= CapacityCost && !IsPlanted) {
                    if (!_currentTrap)
                    {
                        CanRespawn = true;
                        _currentTrap = Instantiate(spawnTrap, transform.position, transform.rotation);
                        OnLooseSunCapacity(CapacityCost);
                        AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                    }
                    else {
                        _currentTrap.transform.position = transform.position;
                        OnLooseSunCapacity(CapacityCost);
                        AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersVoices);
                    }
                    RumbleManager.Instance.RumblePulse(Gamepad);
                }
        }

        private void Respawn()
        {
            if (CanRespawn)
            {
                Rb.MovePosition(_currentTrap.transform.position);
                Destroy(_currentTrap);
                GetRevive();
                CanRespawn = false;
                _isRespawning = false;
            }
            else
            {
                GameManager.Instance.WinVerification();
            }
         
        }

        protected override void TakeHit()
        {
            if (_isRespawning) return;
            base.TakeHit();
        }
        
    }
}