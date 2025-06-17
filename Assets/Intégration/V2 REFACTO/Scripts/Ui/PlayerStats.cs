using Intégration.V1.Scripts.Game.Characters;
using Michael.Scripts.Controller;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Identification")]
    public int playerIndex;
    public int characterIndex;
    public bool IsTurtle { get; set; }
    private TurtleController TurtleController { get; set; }
    private FlowerController FlowerController { get; set; }

    [Header("Stats communes")]
    public int sunsCollected;
    public string title;

    [Header("Stats Tortue")]
    public int trapsPlaced;
    public int flowersEliminated;

    [Header("Stats Fleurs")]
    public int flowersRevived;
    public float timeSpentHidden;


    private void Start()
    {
        InitializeRole();
        
    }

    private void InitializeRole()
    {
        if (TryGetComponent<TurtleController>(out var turtle))
        {
            TurtleController = turtle;
            IsTurtle = true;
        }
        else if (TryGetComponent<FlowerController>(out var flower))
        {
            FlowerController = flower;
            IsTurtle = false;
        }
        else
        {
            Debug.LogWarning($"[PlayerData] Aucun rôle détecté sur {gameObject.name}");
        }
    }
}
