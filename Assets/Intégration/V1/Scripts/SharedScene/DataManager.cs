using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Intégration.V1.Scripts.SharedScene
{
    public class DataManager : MonoBehaviourSingleton<DataManager>
    {
        public enum MapChoice
        {
           Garden,
           Laboratory,
        }

        [System.Serializable]
        public class PlayerInfo
        {
            public int prefabIndex;
            public InputDevice device;
        }
        public Dictionary<int, PlayerInfo> PlayerChoice = new();

        public GameObject loadingScreen;
        public bool CanVibrate = true;
        public MapChoice CurrentMap = MapChoice.Garden;
    }
}
