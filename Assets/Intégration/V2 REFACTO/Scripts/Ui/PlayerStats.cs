using Intégration.V1.Scripts.Game.Characters;
using Michael.Scripts.Controller;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Serialization;

public class PlayerStats : MonoBehaviour
{
    [Header("Identification")] public int playerIndex;
    public int characterIndex;
    public bool IsTurtle { get; set; }

    [Header("Stats communes")] public int sunsCollected = 0;

    [Header("Stats Tortue")] public int trapsPlaced = 0;
    public int flowersScanned = 0;

    [Header("Stats Fleurs")] public int flowersRevived;
    public float timeSpentHidden = 0;
    public int deathNumber = 0;


    public string GetTrophyTitle()
    {
        if (IsTurtle)
        {
            switch (LocalizationSettings.SelectedLocale.Identifier.Code)
            {
                case "fr":
                {
                    if (flowersScanned > 3) return "oeil de Lynx";
                    if (trapsPlaced >= 6) return "Piegeur Compulsif";
                    if (sunsCollected >= 12f) return "Crameur de Soleil";
                    if (sunsCollected < 7f) return "Tortue econome";

                    return "Tortue novice";
                }
                case "en":
                {
                    if (flowersScanned > 3) return "Eagle Eye";
                    if (trapsPlaced >= 6) return "Mine Maniac";
                    if (sunsCollected >= 12f) return "Sun Burner";
                    if (sunsCollected < 7f) return "Efficient Turtle";

                    return "Rookie Turtle";
                }
            }
        }
        else
        {
            if (LocalizationSettings.SelectedLocale.Identifier.Code == "fr")
            {
                if (deathNumber < 1) return "Heroique";
                if (flowersRevived >= 2) return "Altruiste";
                if (sunsCollected >= 12f) return "Addict aux Rayons";
                if (timeSpentHidden >= 30f) return "Timide";

                return "Fleur novice";
            }
            else if ((LocalizationSettings.SelectedLocale.Identifier.Code == "en"))
            {
                if (deathNumber < 1) return "Heroic";
                if (flowersRevived >= 2) return "Altruist";
                if (sunsCollected >= 12f) return "Sunlight Addict";
                if (timeSpentHidden >= 30f) return "Wallflower";

                return "Rookie Flower";
            }
            return "Rookie Flower";
        }

        return null;
    }


}
