using System;
using System.Collections.Generic;
using DG.Tweening;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Controller;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Intégration.V1.Scripts.Game.Characters
{
    public abstract class FlowerController : CharacterController
    {
        public event Action<int> OnSunChanged;
        public event Action<bool> OnDeathChanged;
        
        public bool CanRespawn;
        private int _sun;
        public int Sun
        {
            get => _sun;
            set
            {
                int clamped = Mathf.Clamp(value, 0, maxSun);
                if (_sun != clamped)
                {
                    _sun = clamped;
                    OnSunChanged?.Invoke(_sun);
                }
            }
        }

        public int maxSun = 3;
        public int CapacityCost = 2;
        public bool canReanimate;
        public FlowerController deadFlowerController;
        public bool IsPlanted = false;
        public bool isInvincible = false;
        public bool isUnhittable = false;
        public bool isUnstoppable = false;
        public bool IsStunned;
        public bool isDead;
        public bool isCharging;
        public float reanimateTimer = 0;
        public VisualEffect GatherEnergy;
        [SerializeField] private float reanimateDuration = 1;
        [SerializeField] protected GameObject deadModel;
        [SerializeField] protected GameObject aliveModel;
        [SerializeField] protected Collider aliveModelCollider;
        [SerializeField] private float stunDuration = 3f;
        [SerializeField] private float stunTimer = 0;
        [SerializeField] private ParticleSystem explosionParticleSystem;
        [SerializeField] private ParticleSystem stunParticleSystem;
        [SerializeField] private float plantingCooldown = 0.7f;
        [SerializeField] private GameObject aliveArrowUI;
        [SerializeField] private VisualEffect ReviveVFX;
        [SerializeField] private GameObject dirt;
        private float currentPlantingCooldown = 0f;
        private Sequence _playerIndexSequence;

        [Header("IndexUi")]
        [SerializeField] private Image PlayerIndexUi;
        [SerializeField] private Image PlayerIndexBackground;
        [SerializeField] private CanvasGroup _playerIndexCG;
        [SerializeField] private TextMeshProUGUI _playerIndexText;
        
        [Header("Dead UI")]
        [SerializeField] private GameObject deadArrowUI;
        [SerializeField] private GameObject _redSuns;
        [SerializeField] private GameObject _sunLayoutGroup;
        [SerializeField] private Image reviveChargingIcon;
        [SerializeField] private List<Image> sunImage;
        [SerializeField] private Sprite fullSun;
        [SerializeField] private Sprite emptySun;
        
        private Sequence _shakeSequence;
        private Renderer[] _renderers;
        private void OnEnable()
        {
            GameManager.Instance.OnFlowersWin += WinAnimation;
        }

        private void OnDisable()
        {
            GameManager.Instance.OnFlowersWin -= WinAnimation;
        }

        protected virtual void Start()
        {
            base.Start();
            _playerStats = GetComponent<PlayerStats>();
            _playerStats.IsTurtle = false;
            _playerStats.playerIndex = PlayerIndex;
            _playerStats.characterIndex = characterIndex;
            _renderers = GetComponentsInChildren<Renderer>(true);

            ShowPlayerIndex();
            StartAnimation();
            
        }

        private void ShowPlayerIndex()
        {
            _playerIndexText.text = LocalizationSettings.SelectedLocale.Identifier.Code switch
            {
                "fr" => "J" + (PlayerIndex + 1),
                "en" => "P" + (PlayerIndex + 1),
                _ => _playerIndexText.text
            };

            _playerIndexSequence =  DOTween.Sequence();
            PlayerIndexBackground.color = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.7f, 1f);
           
            _playerIndexSequence.Append(_playerIndexCG.DOFade(1,1f));
            _playerIndexSequence.Join(PlayerIndexUi.rectTransform.DOAnchorPosY(0f, 1f));
            _playerIndexSequence.AppendInterval(1);
            _playerIndexSequence.Append(_playerIndexCG.DOFade(0,1f));
        }

        private void StartAnimation()
        {
            if (deadArrowUI)
            {
                deadArrowUI.transform.DOLocalMoveY(-0.5f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
            }
        }

        public void NotEnoughSunEffect()
        {
            _shakeSequence.Kill();
            _shakeSequence = DOTween.Sequence();

            _redSuns.SetActive(true);
            _sunLayoutGroup.SetActive(false);
            _shakeSequence.Join(_redSuns.transform.DOScale(1.1f, 0.25f));
            _shakeSequence.OnComplete(() =>
            {
                _shakeSequence.Join(_redSuns.transform.DOShakePosition(1f, 0.25f));
               _redSuns.SetActive(false);
               _sunLayoutGroup.SetActive(true);
            });
        }
        

        protected override void FixedUpdate()
        {
            if (!IsStunned)
            {
                Move();
            }
        }

        private void UpdateSunRevive(int sun)
        {
            // Update revive sun visuals
            for (int i = 0; i < sunImage.Count; i++)
            {
                if (i < sun)
                {
                    sunImage[i].sprite = fullSun;
                    sunImage[i].transform.DOScale(1.2f, 0.3f);
                }
                else
                {
                    sunImage[i].sprite = emptySun;
                    sunImage[i].transform.DOScale(1f, 0.3f);
                }
            }
        }

        private void EnableDeathUI(int sun)
        {
            //reviveChargingIcon.gameObject.SetActive(true);
            _sunLayoutGroup.SetActive(true);
            //deadArrowUI.SetActive(false);
            
        }
        
        private void DisableDeathUI()
        {
            //reviveChargingIcon.gameObject.SetActive(false);
            _sunLayoutGroup.SetActive(false);
            //deadArrowUI.SetActive(false);
        }

        protected override void Update()
        {
            
            _animator.SetFloat("Velocity", Rb.linearVelocity.magnitude);
            if (GameManager.Instance.GameFinished)
            {
                deadArrowUI.SetActive(false);
            }
          
            if (deadFlowerController)
            {
                if (isCharging)
                {
                    deadFlowerController.reviveChargingIcon.fillAmount = 0;
                    reanimateTimer += Time.deltaTime;
                    deadFlowerController.reviveChargingIcon.fillAmount = reanimateTimer / reanimateDuration;

                    if (reanimateTimer >= reanimateDuration + 0.1f)
                    {
                        ThirdCapacity();
                        isCharging = false;
                        reanimateTimer = 0;
                        
                    }
                }
                else
                {
                    DOTween.To(() => deadFlowerController.reviveChargingIcon.fillAmount,
                        value => deadFlowerController.reviveChargingIcon.fillAmount = value, 0f, 0.3f);
                }
            }
            

            if (IsStunned)
            {
                stunTimer += Time.deltaTime;
                if (stunTimer >= stunDuration)
                {
                    stunParticleSystem.gameObject.SetActive(false);
                    stunParticleSystem.Clear();
                    IsStunned = false;
                    stunTimer = 0;
                    _animator.SetBool("IsDizzy", false);
                }
            }

            if (currentPlantingCooldown > 0)
            {
                currentPlantingCooldown -= Time.deltaTime;
            }


            if (IsPlanted && !GameManager.Instance.GameFinished && !PauseControlller.IsPaused)
            {
                _playerStats.timeSpentHidden += Time.deltaTime;
            }
           
        }

        public override void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !IsStunned && !PauseControlller.IsPaused)
            {
                MainCapacity();
                
            }
        }


        public override void OnSecondaryCapacity(InputAction.CallbackContext context)
        {
            if (context.started && !IsStunned && !PauseControlller.IsPaused)
            {
                if (!IsStunned && currentPlantingCooldown <= 0)
                {
                    if (!IsPlanted)
                    {
                        SecondaryCapacity();
                    }
                    else
                    {
                        GetUnplanted();
                    }
                }
            }
        }

        protected override void SecondaryCapacity()
        {
            // SE PLANTER DANS LE SOL 
            GetPlanted();
            currentPlantingCooldown = plantingCooldown;
        }

        private void GetUnplanted()
        {
            if (IsPlanted)
            {
                Instantiate(dirt, transform.localPosition, quaternion.identity);
                IsPlanted = false;
                Rb.isKinematic = false;
                _animator.SetBool("isPlanted", IsPlanted);
                aliveModelCollider.enabled = true;
                currentPlantingCooldown = plantingCooldown;
                
                AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersPlanted);
                RumbleManager.Instance.RumblePulse(Gamepad);
            }
        }

        public override void OnThirdCapacity(InputAction.CallbackContext context)
        {
            // REANIMATION

            if (canReanimate && !IsStunned && !PauseControlller.IsPaused)
            {
                if (Sun == maxSun)
                {
                    if (context.started)
                    {
                        isCharging = true;
                    }
                    else if (context.canceled)
                    {
                        isCharging = false;
                        reanimateTimer = 0;
                    }
                }
                else
                {
                    deadFlowerController.NotEnoughSunEffect();
                }
             
            }
        }

        protected override void ThirdCapacity() // revive ally 
        {
            _playerStats.flowersRevived++;
            Debug.Log("revive"); 
            Sun = 0;
            deadFlowerController.GetRevive();
            canReanimate = false;
        }

        protected abstract void PassiveCapacity();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Turtle Collider"))
            {
                TakeHit();
            }

            if (other.CompareTag("TurtleTrap"))
            {
                if (!isDead)
                {
                    GameManager.Instance.TurtleTrap.Remove(other.gameObject);
                    Destroy(other.gameObject);
                    GetStunned();
                }
            }

            if (other.CompareTag("Seed"))
            {
                if (isUnhittable) return;
                if (isDead) return;
                
                canReanimate = true;
                deadFlowerController = other.GetComponentInParent<FlowerController>();
                deadFlowerController.EnableDeathUI(_sun);
            }

            if (!other.gameObject.CompareTag("Turtle") || isDead) return;
            TurtleController _turtleController = other.gameObject.GetComponent<TurtleController>();
            if (!_turtleController.destructionMode) return;
            
            GetStunned();
            _turtleController.destructionMode = false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Shield"))
            {
                isInvincible = true;
            }
            if (other.CompareTag("Seed"))
            {
                deadFlowerController = other.GetComponentInParent<FlowerController>();
                deadFlowerController.UpdateSunRevive(Sun);
            }
            
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Seed"))
            {
                canReanimate = false;
                isCharging = false;
                reanimateTimer = 0;
                if (deadFlowerController)
                {
                    deadFlowerController.DisableDeathUI();
                    deadFlowerController.UpdateSunRevive(Sun);
                    deadFlowerController = null;
                }

                if (other.CompareTag("Shield"))
                {
                    isInvincible = false;
                }
            }
        }


        private void GetPlanted()
        {
            IsPlanted = true;
            Rb.isKinematic = true;
            _animator.SetBool("isPlanted", IsPlanted);
            aliveModelCollider.enabled = false;
            
            AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersPlanted);
            RumbleManager.Instance.RumblePulse(Gamepad);
        }

        [ContextMenu("GetStunned")]
        private void GetStunned()
        {
            if (isDead) return;
            explosionParticleSystem.Play();
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleTrapActivated);
            
            if (!isInvincible && !isUnstoppable && !isUnhittable)
            {
                stunParticleSystem.gameObject.SetActive(true);
                stunParticleSystem.Play();
                GetUnplanted();
                _animator.SetBool("IsDizzy", true);
                IsStunned = true;
                
                RumbleManager.Instance.RumblePulse(Gamepad);
            }
        }


        [ContextMenu("TakeHit")]
        protected virtual void TakeHit()
        {
            if (isInvincible || isUnhittable || GameManager.Instance.GameFinished || isDead) return;
            
            Debug.Log("FLOWERS HIT");
            AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.FlowersDeath);

            if (Gamepad != null)
            {
                RumbleManager.Instance.RumblePulse(Gamepad,1,1);
            }
          
                
            aliveModelCollider.enabled = false;
            aliveModel.SetActive(false);
            deadModel.SetActive(true);
            GetComponent<PlayerInput>().currentActionMap = null;
            Sun = 0;
            isDead = true;
            _playerStats.deathNumber++;
            OnDeathChanged?.Invoke(isDead);
            stunParticleSystem.Clear();
            deadArrowUI.SetActive(true);
            
            foreach (var renderer in _renderers)
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("Flower");;
            }

            if (CanRespawn) return;
            
            GameManager.Instance.FlowersAlive.Remove(gameObject);
            GameManager.Instance.WinVerification();
        }

        [ContextMenu("GetRevive")]
        public void GetRevive()
        {
            if ( GameManager.Instance.GameFinished) return;
            if (!isDead) return;

            isInvincible = true; 
            Invoke(nameof(RespawnProtection),2f);
            aliveModelCollider.enabled = true;
            IsPlanted = false;
            GetComponent<PlayerInput>().SwitchCurrentActionMap("Character");
            isDead = false;
            OnDeathChanged?.Invoke(isDead);
            aliveModel.SetActive(true);
            deadModel.SetActive(false);
            ReviveVFX.Play();
            AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.FlowersRevive);
            
            foreach (var renderer in _renderers)
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("Default");;
            }
            
            if (CanRespawn) return;
            GameManager.Instance.FlowersAlive.Add(gameObject);
        }

        private void RespawnProtection()
        {
            isInvincible = false;
        }
        

        protected void OnLooseSunCapacity(int capacityCost)
        {
            Sun -= capacityCost;
        }

        public void AddSun(int amount)
        {
            Sun += amount;
            _playerStats.sunsCollected += amount;
        }

        private void WinAnimation()
        {
            _animator.SetTrigger("Victory");
            _animator.SetInteger("DanceIndex", Random.Range(0, 3));

        }
    }
}