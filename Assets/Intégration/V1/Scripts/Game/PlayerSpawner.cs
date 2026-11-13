using System.Collections.Generic;
using Cinemachine;
using Intégration.V1.Scripts.Game.Characters;
using Intégration.V1.Scripts.SharedScene;
using Intégration.V1.Scripts.UI;
using Michael.Scripts.Controller;
using Michael.Scripts.Manager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using CharacterController = Intégration.V1.Scripts.Game.Characters.CharacterController;

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

                int prefabIndex = playerInfo.prefabIndex;
                if (prefabIndex >= characterPrefabs.Count)
                {
                    continue;
                }

                GameObject prefab = characterPrefabs[prefabIndex];
                Transform spawnPoint = spawnPoints[prefabIndex];

                GameObject character = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);

                GameManager.Instance.Players.Add(character);

                var playerInput = character.GetComponent<PlayerInput>();
                if (playerInput != null && playerInfo.device != null)
                {
                    var userManager = playerInput.GetComponent<InputUserManager>();

                    if (userManager != null)
                    {
                        userManager.Initialize(playerInfo.device,  character.GetComponent<CharacterController>());
                    }
                }

                if (character.CompareTag("Flower"))
                {   var flowerController = character.GetComponent<FlowerController>();
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
                }

                else if (character.CompareTag("Turtle"))
                {
                    GameManager.Instance.Turtle = character;
                    var turtleController = character.GetComponent<TurtleController>();
                    turtleController.PlayerIndex = playerIndex;
                    SeeTroughWall._turtle = character;
                    _targetGroup.AddMember(character.transform, 1.1f, 2.5f);
                }
            }

            if (GameManager.Instance.Turtle == null && GameManager.Instance.FlowersAlive.Count > 0)
            {
                SpawnAITurtle();
            }
        }

        private void SpawnAITurtle()
        {
            int turtleIndex = characterPrefabs.FindIndex(p => p != null && p.CompareTag("Turtle"));
            if (turtleIndex < 0 || turtleIndex >= spawnPoints.Count)
            {
                return;
            }

            GameObject inactiveHolder = new GameObject("AI Turtle Holder");
            inactiveHolder.SetActive(false);

            Transform spawnPoint = spawnPoints[turtleIndex];
            GameObject turtle = Instantiate(characterPrefabs[turtleIndex], spawnPoint.position, Quaternion.identity, inactiveHolder.transform);
            turtle.name = "Turtle (IA)";

            var playerInput = turtle.GetComponent<PlayerInput>();
            if (playerInput != null) playerInput.enabled = false;

            var qteManager = turtle.GetComponent<QteManager>();
            if (qteManager != null) qteManager.IsAIControlled = true;

            var turtleController = turtle.GetComponent<TurtleController>();
            turtleController.PlayerIndex = -1;
            turtleController.IsAIControlled = true;
            turtle.AddComponent<TurtleAI>();

            turtle.transform.SetParent(transform, true);
            Destroy(inactiveHolder);

            GameManager.Instance.Turtle = turtle;
            GameManager.Instance.Players.Add(turtle);
            SeeTroughWall._turtle = turtle;
            _targetGroup.AddMember(turtle.transform, 1.1f, 2.5f);
        }
    }
}
