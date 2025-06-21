using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Intégration.V1.Scripts.SharedScene
{
    public class DataManager : MonoBehaviourSingleton<DataManager>
    {
        [System.Serializable] 
        public class PlayerInfo
        {
            public int prefabIndex;
            public InputDevice device;
        }
        public Dictionary<int, PlayerInfo> PlayerChoice = new(); // stock le choix de personnage et les manettes
        
        public GameObject loadingScreen;
        public static bool CanVibrate = true;


     
       
      
      
        
    }
}