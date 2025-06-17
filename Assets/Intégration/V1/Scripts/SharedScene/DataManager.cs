using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Intégration.V1.Scripts.SharedScene
{
    public class DataManager : MonoBehaviourSingleton<DataManager>
    {
        //public Dictionary<int, int> PlayerChoice = new Dictionary<int, int>();
        public GameObject loadingScreen;
        public static bool CanVibrate = true;
        public static bool UiInWorldSpace = true;

        
        public Dictionary<int, PlayerInfo> PlayerChoice = new();
       
        [System.Serializable]
        public class PlayerInfo
        {
            public int prefabIndex;
            public InputDevice device;
        }
      
        
    }
}