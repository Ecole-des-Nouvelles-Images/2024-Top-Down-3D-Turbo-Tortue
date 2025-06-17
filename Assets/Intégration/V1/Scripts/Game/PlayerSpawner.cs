using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Intégration.V1.Scripts.Game.Characters;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V1.Scripts.UI;
using Michael.Scripts;
using Michael.Scripts.Controller;
using Michael.Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Intégration.V1.Scripts.Game
{
    public class PlayerSpawner : MonoBehaviourSingleton<PlayerSpawner>
    {
        public List<GameObject> characterPrefabs;
        public List<Transform> spawnPoints;
        [SerializeField] private CinemachineTargetGroup _targetGroup;
        [SerializeField] private List<FlowerUI> FlowerUis;
        private int flowerUiIndex = 0;
        

        private void Start()
        {
            
            foreach (var kvp in DataManager.Instance.PlayerChoice)
            {
                int playerIndex = kvp.Key;
                var playerInfo = kvp.Value;

                
                // Sélection du prefab
                int prefabIndex = playerInfo.prefabIndex;
                if (prefabIndex >= characterPrefabs.Count)
                {
                    Debug.LogWarning($"Prefab index {prefabIndex} invalide pour le joueur {playerIndex}");
                    continue;
                }

                GameObject prefab = characterPrefabs[prefabIndex];
                Transform spawnPoint = spawnPoints[prefabIndex];

                GameObject character = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);
                Debug.Log($"[Spawner] Joueur {playerIndex} instancié au point {spawnPoint.position} avec prefab '{prefab.name}'");
                GameManager.Instance.Players.Add(character);

                // ⚙️ Gérer PlayerInput et InputDevice
                var playerInput = character.GetComponent<PlayerInput>();
                if (playerInput != null && playerInfo.device != null)
                {
                    var userManager = playerInput.GetComponent<InputUserManager>();
                    
                    if (userManager != null)
                    {
                        userManager.Initialize(playerInfo.device);
                    }
                    Debug.Log($"[Spawner] InputUser associé à {playerInfo.device} pour le joueur {playerIndex}");

                }
                else
                {
                    Debug.LogWarning($"[Spawner] PlayerInput ou device manquant pour joueur {playerIndex}");
                }

                // 🏵️ Cas spécifique : Flower
                if (character.CompareTag("Flower"))
                {
                    var flowerController = character.GetComponent<FlowerController>();
                    flowerController.characterIndex = prefabIndex;
                    flowerController.PlayerIndex = playerIndex;

                    if (flowerUiIndex < FlowerUis.Count)
                    {
                        FlowerUis[flowerUiIndex].FlowerPlayer = flowerController;
                        FlowerUis[flowerUiIndex].GameObject().SetActive(true);
                        flowerUiIndex++;
                    }

                    GameManager.Instance.FlowersAlive.Add(character);
                    _targetGroup.AddMember(character.transform, 1, 2);
                    Debug.Log("Flower ajouté");
                }

                // 🐢 Cas spécifique : Turtle
                else if (character.CompareTag("Turtle"))
                {
                    GameManager.Instance.Turtle = character;
                    var turtleController = character.GetComponent<TurtleController>();
                    turtleController.PlayerIndex = playerIndex;
                    SeeTroughWall._turtle = character;
                    _targetGroup.AddMember(character.transform, 1.1f, 2.5f);
                    Debug.Log("Turtle ajouté");
                }
            }
            // Optionnel : reset des données si plus utiles
            //DataManager.Instance.PlayerChoice.Clear(
        }
    }
}