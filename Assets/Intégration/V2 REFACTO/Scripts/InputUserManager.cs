using System.Linq;
using Michael.Scripts.Controller;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using CharacterController = Intégration.V1.Scripts.Game.Characters.CharacterController;

public class InputUserManager : MonoBehaviour
{
    public InputDevice AssignedDevice { get; private set; }
    private PlayerInput playerInput;
    private InputUser user;
    private bool userInitialized = false;
    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }

  

    public void Initialize(InputDevice device, CharacterController _characterController)
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        if (device != null && playerInput != null)
        {
            AssignedDevice = device;
            user = InputUser.CreateUserWithoutPairedDevices();
            InputUser.PerformPairingWithDevice(device, user);
            user.AssociateActionsWithUser(playerInput.actions);
            userInitialized = true;


            if (_characterController != null)
            {
                _characterController.Gamepad =  user.pairedDevices.OfType<Gamepad>().FirstOrDefault();
            }
            else
            {
                Debug.Log("FFFFFFF" );
            }

            
            if (_characterController.Gamepad == null)
            {
                Debug.LogWarning($"[Input] Aucun gamepad trouvé pour {user} !");
            }
        }
        else
        {
            Debug.LogWarning("[InputUserManager] Impossible d'initialiser l'utilisateur (device ou PlayerInput manquant).");
        }
    }

    private void OnEnable()
    {
        if (userInitialized && playerInput != null && !playerInput.user.valid)
        {
            if (AssignedDevice != null && !user.pairedDevices.Contains(AssignedDevice))
            {
                InputUser.PerformPairingWithDevice(AssignedDevice, user);
                user.AssociateActionsWithUser(playerInput.actions);
            }
        }
    }

  
}

