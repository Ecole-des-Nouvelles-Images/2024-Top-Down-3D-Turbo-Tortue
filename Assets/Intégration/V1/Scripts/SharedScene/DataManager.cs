using System.Collections.Generic;
using Michael.Scripts.Manager;
using UnityEngine;

namespace Intégration.V1.Scripts.SharedScene
{
    public class DataManager : MonoBehaviourSingleton<DataManager>
    {
        public Dictionary<int, int> PlayerChoice = new Dictionary<int, int>();
        public GameObject loadingScreen;
        public static float SfxVolume = 0.5f;
        public static float MusicVolume = 0.5f;
        public static bool CanVibrate = true;
        public static bool UiInWorldSpace = true;
        //public static bool CharacterSelectionScene = false;
    }
}