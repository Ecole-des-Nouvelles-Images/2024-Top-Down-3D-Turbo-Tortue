using System;
using System.Linq;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Manager;
using Michael.Scripts.Ui;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Intégration.V1.Scripts.Game.Characters
{
    public abstract class CharacterController : MonoBehaviour
    {
        public int PlayerIndex;
        public int characterIndex;
        private static readonly int Run = Animator.StringToHash("Run");
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float idleTreshold = 0.1f;
        protected Vector2 move;
        protected Rigidbody Rb;
        protected Animator _animator;
        protected PlayerStats _playerStats;
        protected PlayerInput _playerInput;
        public Gamepad _gamepad;
        

        
        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _playerInput = GetComponent<PlayerInput>();
            
        }

        protected virtual void Start()
        {
            
        }

        public void DeviceDeconnected()
        {
            RumbleManager.Instance.StopRumbleLoop(_gamepad);
        }
        
        public virtual void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed && !PauseControlller.IsPaused && GameManager.Instance.GameisStarted && !GameManager.Instance.GameFinished)
            {
                Pause();
            }
        }
        
        protected virtual void Pause()
        {
            Debug.Log("JAI APPUY2C USR PAUESE ??");
            PauseControlller.OnGamePaused.Invoke();
        }

        protected virtual void FixedUpdate()
        {
            Move();
        }

        protected virtual void Update()
        {
        }

        #region Move

        public void OnMove(InputAction.CallbackContext context)
        {
            if (!PauseControlller.IsPaused)
            {
                move = context.ReadValue<Vector2>();
            }
        }

        protected virtual void Move()
        {
            Vector3 movement = new Vector3(move.x, 0f, move.y) * moveSpeed;
            if (movement != Vector3.zero)
            {
                Quaternion newRotation = Quaternion.LookRotation(movement, Vector3.up);
                Rb.rotation = Quaternion.Slerp(Rb.rotation, newRotation, 0.15f);
            }

            // Rb.MovePosition(transform.position + new Vector3(movement.x, 2, movement.z) * Time.deltaTime);
            Rb.AddForce(movement * Time.deltaTime, ForceMode.Force);
            // Rb.velocity = new Vector3(movement.x, Rb.velocity.y, movement.z);
        }

        #endregion

        #region Main Capacity

        public virtual void OnMainCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !PauseControlller.IsPaused)
            {
                MainCapacity();
            }
        }

        protected abstract void MainCapacity();

        #endregion

        #region Secondary Capacity

        public virtual void OnSecondaryCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !PauseControlller.IsPaused)
            {
                SecondaryCapacity();
                //RumbleManager.Instance.RumblePulse(0.25f, 1f, 0.25f);
            }
        }

        protected abstract void SecondaryCapacity();

        #endregion

        #region Third Capacity

        public virtual void OnThirdCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !PauseControlller.IsPaused)
            {
                ThirdCapacity();
            }
        }

        protected virtual void ThirdCapacity()
        {
        }

        #endregion


        #region Fourth Capacity

        public virtual void OnFourthCapacity(InputAction.CallbackContext context)
        {
            if (context.performed && !PauseControlller.IsPaused)
            {
                FourthCapacity();
            }
        }

        protected virtual void FourthCapacity()
        {
        }

        #endregion
    }
}