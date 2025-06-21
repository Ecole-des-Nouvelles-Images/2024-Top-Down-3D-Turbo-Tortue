using Intégration.V1.Scripts.Game.Characters;
using Michael.Scripts.Controller;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    [Header("Identification")]
    public int playerIndex;
    public int characterIndex;
    public bool IsTurtle { get; set; }

    [Header("Stats communes")]
    public int sunsCollected = 0;

    [Header("Stats Tortue")]
    public int trapsPlaced = 0;
    public int flowersScanned = 0;

    [Header("Stats Fleurs")]
    public int flowersRevived;
    public float timeSpentHidden = 0;
    public int deathNumber= 0; 
    

    public string GetTrophyTitle()
    {
        if (IsTurtle)
        {
            if (flowersScanned > 3) return "oeil de Lynx";
            if (trapsPlaced >= 6) return "Piegeur Compulsif";
            if (sunsCollected >= 12f) return "Crameur de Soleil";
            if (sunsCollected < 7f) return "Tortue econome";
            
            return "Tortue novice";
        }
        else
        {
            if (deathNumber < 1) return "Heroique";
            if (flowersRevived >= 2) return "Altruiste";
            if (sunsCollected >= 12f) return "Addict aux Rayons";
            if (timeSpentHidden >= 30f) return "Timide";
            
            return "Fleur novice";
        }
    }
    
    
    
}
