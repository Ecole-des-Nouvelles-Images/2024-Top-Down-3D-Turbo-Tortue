using System.Collections;
using UnityEngine;

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
            if (_currentTrap) {
                Respawn();
            }
            else
            {
                CanRespawn = false;
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
                }
        }

        private void Respawn()
        {
            if (isDead && !_isRespawning)
            {
                StartCoroutine(RespawnWaiter());
            }
        }

        IEnumerator RespawnWaiter() {
            _isRespawning = true;
            yield return new WaitForSeconds(respawnDelay);
            Rb.MovePosition(_currentTrap.transform.position);
            Destroy(_currentTrap);
            GetRevive();
            CanRespawn = false;
           _isRespawning = false;
        }
    }
}