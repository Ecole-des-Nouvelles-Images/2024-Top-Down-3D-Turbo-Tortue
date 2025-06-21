using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.Game;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
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

    public enum TurtleState
    {
        Default,
        Charging,
        Dashing,
        Boosting,
        Scanning,
        Dead
    }

    public class TurtleController : CharacterController
    {
        public TurtleState CurrentState = TurtleState.Default;
        public bool destructionMode;

        [Header("General References")]
        [SerializeField] private Collider _attackCollider;
        [SerializeField] private GameObject SpeedParticle;
        [SerializeField] private Material dashMaterial;

        [Header("Capacities Cost")]
        [SerializeField] private float _dashLvl1Cost = 10f;
        [SerializeField] private float _dashLvl2Cost = 20f;
        [SerializeField] private float _biteCost = 10f;
        [SerializeField] private float _trapCost = 10f;
        [SerializeField] private float _scanCost = 10f;

        [Header("Boost")]
        [SerializeField] private float boosterMultiplier = 1.2f;
        [SerializeField] private Color boostColor;
        [SerializeField] private Color normalColor;

        [Header("Charging & Dashing")]
        [SerializeField] private List<DashLevel> dashLevels;
        [SerializeField] private Material materialToUpdate;
        [SerializeField] private GameObject chargingParticules;
        [SerializeField] private GameObject chargingSmokeParticules;

        [Header("Scanning")]
        [SerializeField] private float scanTime;
        [SerializeField] private float scanRange;
        [SerializeField] private float scanDuration;
        [SerializeField] private GameObject scanSphereArea;

        [Header("Trap")]
        [SerializeField] private GameObject TrapPrefab;
        [SerializeField] private Transform TrapSpawn;
        
        
        private float _chargeTime;
        private int _currentDashLevel = -1;
        private float _normalSpeed;
        private Vector3 _lastDashDirection;
        
        private ScannerArea _scannerArea;

        protected override void Awake()
        {
            base.Awake();
            _scannerArea = scanSphereArea.GetComponent<ScannerArea>();
            _normalSpeed = moveSpeed;
        }

        protected virtual void Start()
        {
            base.Start();
            _attackCollider.enabled = false;
            _playerStats = GetComponent<PlayerStats>();
            _playerStats.IsTurtle = true;
            _playerStats.playerIndex = PlayerIndex;
        }

        private void OnEnable()
        {
            QteManager.Instance.OnQteFinished += TurnAnimation;
            BatteryManager.OnSunCollected += SunCollected;
            _scannerArea.OnFlowerDetected += FlowerScanned;
            PauseControlller.OnGamePaused += ClearAllInput;
            GameManager.Instance.OnTurtleDead += SetDeadState;
        }

        private void OnDisable()
        {
            QteManager.Instance.OnQteFinished -= TurnAnimation;
            _scannerArea.OnFlowerDetected -= FlowerScanned;
            BatteryManager.OnSunCollected -= SunCollected;
            PauseControlller.OnGamePaused -= ClearAllInput;
            GameManager.Instance.OnTurtleDead -= SetDeadState;
        }

       
        private void ClearAllInput()
        {
            StopBoost();
            StopCharging();
        }

        private void SunCollected(int amount) => _playerStats.sunsCollected++;

        private void FlowerScanned()
        {
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleScanAlert);
            _playerStats.flowersScanned++;
        }

        private void TurnAnimation() => _animator.SetBool("QteSuccess", true);

        protected override void FixedUpdate()
        {
            if (CurrentState == TurtleState.Default || CurrentState == TurtleState.Boosting || CurrentState == TurtleState.Scanning)
                Move();
        }

        protected override void Update()
        {
            UpdateState();
            if (CurrentState != TurtleState.Charging) UpdateNitroVisuals(CurrentState == TurtleState.Boosting);
            _animator.SetFloat("Velocity", Rb.velocity.magnitude);
        }

        private void UpdateState()
        {

            switch (CurrentState)
            {
                case TurtleState.Dead:
                    AudioManager.Instance.StopLoopingSfx();
                    break;
                case TurtleState.Default:
                  TurtleDefaultState();
                    break;
                case TurtleState.Charging:
                    UpdateCharging();
                    break;
                case TurtleState.Dashing:
                    UpdateDashing();
                    break;
                case TurtleState.Scanning:
                    UpdateScanning();
                    break;
                case TurtleState.Boosting:
                    break;
              
            }
        }

        private void TurtleDefaultState()
        {
            if (PauseControlller.IsPaused || !GameManager.Instance.GameisStarted) return;
          
            _animator.SetBool("IsDashing",false);
           // AudioManager.Instance.PlayLoopSfx(AudioManager.Instance.ClipsIndex.TurtleReactor,0.2f);
        }

        private void SetDeadState()
        {
            _animator.SetBool("IsDead", true);
            _playerInput.enabled = false;
            materialToUpdate.SetColor("_EmissionColor", Color.black);
            StopAllActions();
            
        }

        private void StopAllActions()
        {
            StopCharging();
            StopBoost();
            CurrentState = TurtleState.Dead;
        }

        private void UpdateNitroVisuals(bool isActive)
        {
            SpeedParticle.SetActive(isActive);
            dashMaterial.SetColor("_EmissionColor", isActive ? boostColor : normalColor);
            materialToUpdate.SetColor("_EmissionColor", isActive ? dashLevels[^1].emissionColor : dashLevels[0].emissionColor);
            BatteryManager.NitroActivate = isActive;
        }

        public void OnBooster(InputAction.CallbackContext context)
        {
            if (context.started && CanBoost())
            {
                StartBoost();
            }
            else if (context.canceled && CurrentState == TurtleState.Boosting)
            {
                StopBoost();
            }
        }

        private bool CanBoost() => CurrentState == TurtleState.Default && !GameManager.Instance.TurtleIsDead && !PauseControlller.IsPaused;

        private void StartBoost()
        {
            /*if (move.magnitude <= 0.1f)
                _lastDashDirection = (_lastDashDirection != Vector3.zero) ? _lastDashDirection : transform.forward;
            else
                _lastDashDirection = new Vector3(move.x, 0f, move.y).normalized;*/

            moveSpeed *= boosterMultiplier;
            
            AudioManager.Instance.StopLoopingSfx();
            AudioManager.Instance.PlayLoopSfx(AudioManager.Instance.ClipsIndex.TurtleReactorNitro, 0.5f);
            RumbleManager.Instance.RumbleLoop(_gamepad, 0.2f, 0.2f);
            CurrentState = TurtleState.Boosting;
            
        }

        private void StopBoost()
        {
            _lastDashDirection = Vector3.zero;
            moveSpeed = _normalSpeed;
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleEndNitro, 0.5f);
            AudioManager.Instance.StopLoopingSfx();
            RumbleManager.Instance.StopRumbleLoop(_gamepad);
            move = Vector2.zero;
            if (CurrentState == TurtleState.Boosting)
                CurrentState = TurtleState.Default;
        }

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.started && CanStartCharging())
            {
                StartCharging();
            }
            else if (context.canceled && CurrentState == TurtleState.Charging)
            {
                StopCharging();
                MainCapacity();
            }
        }

        protected override void MainCapacity()
        {
            PerformDash();
        }

        private bool CanStartCharging() => CurrentState == TurtleState.Default && !GameManager.Instance.TurtleIsDead && !PauseControlller.IsPaused;

        private void StartCharging()
        {
            _chargeTime = 0f;
            _currentDashLevel = -1;
            CurrentState = TurtleState.Charging;
            RumbleManager.Instance.RumbleLoop(_gamepad, 0.2f, 0.2f);
        }

        private void StopCharging()
        {
            chargingParticules.SetActive(false);
            chargingSmokeParticules.SetActive(false);
            _currentDashLevel = -1;
            RumbleManager.Instance.StopRumbleLoop(_gamepad);
            if (CurrentState == TurtleState.Charging)
                CurrentState = TurtleState.Default;
        }

        private void UpdateCharging()
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
            if (level.sfx) AudioManager.Instance.PlaySound(level.sfx);

            if (levelIndex >= 1)
            {
                chargingSmokeParticules.SetActive(true);
                chargingParticules.SetActive(true);
                AudioManager.Instance.StopLoopingSfx();
                AudioManager.Instance.PlayLoopSfx(AudioManager.Instance.ClipsIndex.TurtleTurnAround);
            }
            else
            {
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleStartTurn, 0.5f);
            }
        }

        private void PerformDash()
        {
            int dashLevel = GetCurrentDashLevel();
            if (dashLevel < 0 || dashLevel >= dashLevels.Count) return;

            DashLevel level = dashLevels[dashLevel];
            Vector3 dashDirection = move.magnitude > 0.1f ? new Vector3(move.x, 0f, move.y) : transform.forward;
            if (_lastDashDirection != Vector3.zero) dashDirection = _lastDashDirection;

            Rb.AddForce(level.power * level.timeThreshold * dashDirection.normalized, ForceMode.Impulse);
            _animator.SetBool("IsDashing", true);
            Rb.rotation = Quaternion.LookRotation(dashDirection);

            if (dashLevel == 1) BatteryManager.OnBatteryDecrease.Invoke(_dashLvl1Cost);
            if (dashLevel == 2) BatteryManager.OnBatteryDecrease.Invoke(_dashLvl2Cost);

            Invoke(nameof(DelayDestructionMode), 1f);
            CurrentState = TurtleState.Dashing;
        }

        private void UpdateDashing()
        {
            if (Rb.velocity.magnitude < 0.01f)
            {
                _animator.SetBool("IsDashing", false);
                _lastDashDirection = Vector3.zero;
                //destructionMode = false;
                CurrentState = TurtleState.Default;
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleDash, 0.5f);
                AudioManager.Instance.StopLoopingSfx();
            }
        }

        private void DelayDestructionMode() => destructionMode = false;

        protected override void Move()
        {
            Vector3 direction = Vector3.zero;

            if (move.magnitude > 0.1f)
            {
                // Joystick actif → on suit le joystick
                direction = new Vector3(move.x, 0f, move.y).normalized;
                _lastDashDirection = direction; // Met à jour la direction utilisée en boost
            }
            else if (CurrentState == TurtleState.Boosting && _lastDashDirection != Vector3.zero)
            {
                // Pas de joystick, mais boost actif → avancer dans la dernière direction connue
                direction = _lastDashDirection;
            }
            else
            {
                // Aucun mouvement
                return;
            }

            Vector3 movement = direction * moveSpeed;

            if (movement != Vector3.zero)
            {
                var targetRot = Quaternion.LookRotation(movement, Vector3.up);
                Rb.rotation = Quaternion.Slerp(Rb.rotation, targetRot, 0.15f);
            }

            Rb.AddForce(movement * Time.deltaTime, ForceMode.Force);
        
        }

        protected override void SecondaryCapacity()
        {
            if (CurrentState == TurtleState.Dashing) return;

            EnableAttackCollider();
            Invoke(nameof(DisableAttackCollider), 0.4f);
            _animator.SetTrigger("Attack");
            BatteryManager.OnBatteryDecrease.Invoke(_biteCost);
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleBite);
        }

        private void EnableAttackCollider() => _attackCollider.enabled = true;
        private void DisableAttackCollider() => _attackCollider.enabled = false;

        protected override void ThirdCapacity()
        {
            if (GameManager.Instance.TurtleTrap.Count <= 2)
            {
                _playerStats.trapsPlaced++;
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleSpawnTrap, 0.5f);
                GameObject trap = Instantiate(TrapPrefab, TrapSpawn.position, TrapSpawn.rotation);
                GameManager.Instance.TurtleTrap.Add(trap);
                BatteryManager.OnBatteryDecrease.Invoke(_trapCost);
                RumbleManager.Instance.RumblePulse(_gamepad);
            }
        }

        protected override void FourthCapacity()
        {
            if (CurrentState is TurtleState.Scanning or not TurtleState.Default) return;

            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleScan);
            scanSphereArea.transform.DOScale(scanRange, scanDuration);
            BatteryManager.OnBatteryDecrease.Invoke(_scanCost);
            RumbleManager.Instance.RumblePulse(_gamepad);
            CurrentState = TurtleState.Scanning;
        }

        private void UpdateScanning()
        {
            scanTime += Time.deltaTime;
            if (scanTime >= scanDuration)
            {
                scanTime = 0;
                scanSphereArea.transform.DOScale(0, 0f);
                if (CurrentState == TurtleState.Scanning)
                    CurrentState = TurtleState.Default;
            }
        }
    }

}

