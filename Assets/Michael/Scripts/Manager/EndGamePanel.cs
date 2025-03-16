using System;
using System.Collections.Generic;
using Intégration.V1.Scripts.SharedScene;
using Michael.Scripts.Ui;
using UnityEngine;

namespace Michael.Scripts.Manager
{
    public class EndGamePanel : MonoBehaviourSingleton<EndGamePanel>

    {

        public void LoadCharacterSelectionScene(string sceneName)
        {
            DataManager.Instance.PlayerChoice.Clear();
            //DataManager.CharacterSelectionScene = true;
            CustomSceneManager.Instance.LoadScene(sceneName);
           
        }
        
        public void ReloadScene(string sceneName)
        {
            CustomSceneManager.Instance.ReloadActiveScene();
        }
        
        public void LoadMenuScene(string sceneName)
        {
            DataManager.Instance.PlayerChoice.Clear();
            CustomSceneManager.Instance.LoadScene(sceneName);
        }
        

        
    }
}