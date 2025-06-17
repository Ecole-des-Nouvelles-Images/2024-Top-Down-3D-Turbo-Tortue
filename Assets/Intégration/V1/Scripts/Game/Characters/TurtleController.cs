using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using Noah.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using CharacterController = Intégration.V1.Scripts.Game.Characters.CharacterController;

namespace Michael.Scripts.Controller
{
    [System.Serializable]
    public struct DashLevel
    {
        public float timeThreshold;
        public float power;
        public Color emissionColor;
        public AudioClip sfx;
    }

    public class TurtleController : CharacterController
    {
        public bool destructionMode;

        [Header("General References")] [SerializeField]
        private Collider _attackCollider;

        [SerializeField] private GameObject SpeedParticle;
        [SerializeField] private Material dashMaterial;

        [Header("Boost")] [SerializeField] private float boosterMultiplier = 1.2f;
        [SerializeField] private bool nitroActivate;
        [SerializeField] private Color boostColor;
        [SerializeField] private Color normalColor;

        [Header("Charging & Dashing")] [SerializeField]
        private List<DashLevel> dashLevels;

        [SerializeField] private Material materialToUpdate;
        [SerializeField] private GameObject chargingParticules;
        [SerializeField] private GameObject chargingSmokeParticules;

        private int _currentDashLevel = -1;
        private float _chargeTime;
        private bool _isCharging;
        private bool _isDashing;
        private Vector3 _lastDashDirection;
        private float _normalSpeed;

        [Header("Scanning")] [SerializeField] private float scanTime;
        [SerializeField] private float scanRange;
        [SerializeField] private float scanDuration;
        [SerializeField] private GameObject scanSphereArea;
        private bool _isScanning;

        [Header("Trap")] [SerializeField] private GameObject TrapPrefab;
        [SerializeField] private Transform TrapSpawn;

        [Header("spawn references")] 
        [SerializeField] private ParticleSystem _crashParticules;
        [SerializeField] private Vector3 _spawnPosition;
        private QteManager _qteManager;
        
        private void Start()
        {
            QteManager.Instance.OnQteFinished += TurnAnimation;
            _attackCollider.enabled = false;
            _normalSpeed = moveSpeed;
            _qteManager = GetComponentInChildren<QteManager>();
        }

        public void EnableTurtle()
        {
            transform.position = _spawnPosition;
            _qteManager.StartQTE();
            _crashParticules.Play();
        }

        private void TurnAnimation()
        {
            _animator.SetBool("QteSuccess", true);
        }

        protected override void FixedUpdate()
        {
            if (!_isDashing && !_isCharging)
                Move();
        }

        protected override void Update()
        {
            if (nitroActivate)
            {
                SpeedParticle.SetActive(true);
                materialToUpdate.SetColor("_EmissionColor", dashLevels[^1].emissionColor);
                dashMaterial.SetColor("_EmissionColor", boostColor);
                BatteryManager.NitroActivate = true;
            }
            else
            {
                SpeedParticle.SetActive(false);
                dashMaterial.SetColor("_EmissionColor", normalColor);
                BatteryManager.NitroActivate = false;
            }

            DashingUpdate();
            ScanningUpdate();
            _animator.SetFloat("Velocity", Rb.velocity.magnitude);

            if (GameManager.Instance.TurtleIsDead)
            {
                _animator.SetBool("IsDead", true);
                GetComponent<PlayerInput>().enabled = false;
                materialToUpdate.SetColor("_EmissionColor", Color.black);
            }
            else if (!_isCharging && !nitroActivate)
            {
                materialToUpdate.SetColor("_EmissionColor", dashLevels[0].emissionColor);
            }
        }

        public void OnBooster(InputAction.CallbackContext context)
        {
            if (PauseControlller.IsPaused) return;
            if (_isCharging) return;
            
            if (context.started)
            {
                if (nitroActivate) return;
                moveSpeed *= boosterMultiplier;
                AudioManager.Instance.PlayLoopSfx(AudioManager.Instance.ClipsIndex.TurtleNitro);
                nitroActivate = true;
            }
            else if (context.canceled)
            {
                moveSpeed = _normalSpeed;
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleEndNitro);
                AudioManager.Instance.StopLoopingSfx();
                nitroActivate = false;
            }
        }

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (PauseControlller.IsPaused) return;

            if (context.started)
            {
                if (_isCharging) return;
                StartCharging();
            }
            else if (context.canceled)
            {
                StopCharging();
                MainCapacity();
                AudioManager.Instance.StopLoopingSfx();
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleEndNitro);
            }
        }

        protected override void MainCapacity()
        {
            if (_isDashing) return;

            Vector3 dashDirection = move.magnitude > 0.1f ? new Vector3(move.x, 0f, move.y) : transform.forward;
            if (_lastDashDirection != Vector3.zero) dashDirection = _lastDashDirection;

            int dashLevel = GetCurrentDashLevel();
            if (dashLevel < 0 || dashLevel >= dashLevels.Count) return;

            DashLevel level = dashLevels[dashLevel];
            float force = level.power * level.timeThreshold;
            Rb.AddForce(force * dashDirection.normalized, ForceMode.Impulse);

            _animator.SetBool("IsDashing", true);
            _isDashing = true;

            if (dashDirection != Vector3.zero)
                Rb.rotation = Quaternion.LookRotation(dashDirection);

            if (dashLevel == 1) BatteryManager.OnBatteryDecrease.Invoke(10);
            if (dashLevel == 2) BatteryManager.OnBatteryDecrease.Invoke(20);

            Invoke(nameof(DelayDestructionMode), 1);
        }

        private void DashingUpdate()
        {
            if (_isDashing && Rb.velocity.magnitude < 0.01f)
            {
                _isDashing = false;
                _animator.SetBool("IsDashing", false);
                chargingSmokeParticules.SetActive(false);
                chargingParticules.SetActive(false);
                _lastDashDirection = Vector3.zero;
                _currentDashLevel = -1;
                return;
            }

            if (_isCharging)
            {
                _animator.SetBool("IsDashing", true);
                _animator.SetFloat("DashTimer", _chargeTime);
                _chargeTime += Time.deltaTime;

                if (move.magnitude > 0.5f)
                    _lastDashDirection = new Vector3(move.x, 0f, move.y);

                int levelIndex = GetCurrentDashLevel();
                ApplyDashLevelEffects(levelIndex);

                if (levelIndex >= 2)
                    destructionMode = true;
            }
        }

        private int GetCurrentDashLevel()
        {
            for (int i = dashLevels.Count - 1; i >= 0; i--)
            {
                if (_chargeTime >= dashLevels[i].timeThreshold)
                    return i;
            }

            return -1;
        }

        private void ApplyDashLevelEffects(int levelIndex)
        {
            if (levelIndex == _currentDashLevel || levelIndex < 0 || levelIndex >= dashLevels.Count) return;

            _currentDashLevel = levelIndex;
            DashLevel level = dashLevels[levelIndex];

            materialToUpdate.SetColor("_EmissionColor", level.emissionColor);
            
            if (level.sfx) { AudioManager.Instance.PlaySound(level.sfx); }
            
            if (levelIndex >= 1)
            {
                chargingSmokeParticules.SetActive(true);
                chargingParticules.SetActive(true);
                AudioManager.Instance.PlayLoopSfx(AudioManager.Instance.ClipsIndex.TurtleTurn);
            }
        }

        private void StartCharging()
        {
            _isCharging = true;
            _chargeTime = 0f;
        }

        private void StopCharging()
        {
            if (_currentDashLevel == -1)
            {
                // Si pas assez chargé pour dash → reset l’état visuel
                _animator.SetBool("IsDashing", false);
                chargingParticules.SetActive(false);
                chargingSmokeParticules.SetActive(false);
                _lastDashDirection = Vector3.zero;
            }
            _isCharging = false;
        }

        private void DelayDestructionMode()
        {
            destructionMode = false;
        }

        protected override void SecondaryCapacity()
        {
            if (_isDashing) return;

            EnableAttackCollider();
            Invoke(nameof(DisableAttackCollider), 0.4f);
            _animator.SetTrigger("Attack");
            BatteryManager.OnBatteryDecrease.Invoke(10);
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleBite);
        }

        private void EnableAttackCollider() => _attackCollider.enabled = true;
        private void DisableAttackCollider() => _attackCollider.enabled = false;

        protected override void ThirdCapacity()
        {
            if (GameManager.Instance.TurtleTrap.Count <= 2)
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleSpawnTrap);
                GameObject trap = Instantiate(TrapPrefab, TrapSpawn.position, TrapSpawn.rotation);
                GameManager.Instance.TurtleTrap.Add(trap);
                BatteryManager.OnBatteryDecrease.Invoke(10);
            }
        }

        protected override void FourthCapacity()
        {
            if (_isScanning) return;

            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleScan);
            scanSphereArea.transform.DOScale(scanRange, 3f);
            _isScanning = true;
            BatteryManager.OnBatteryDecrease.Invoke(10);
        }

        private void ScanningUpdate()
        {
            if (!_isScanning) return;

            scanTime += Time.deltaTime;
            if (scanTime >= scanDuration)
            {
                _isScanning = false;
                scanTime = 0;
                scanSphereArea.transform.DOScale(0, 0);
            }
        }
    }
}