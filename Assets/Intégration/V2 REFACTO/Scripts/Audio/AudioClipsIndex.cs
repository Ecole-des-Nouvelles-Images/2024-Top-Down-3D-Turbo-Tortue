using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AudioClipsIndex
{
    // UI
    [field: SerializeField] public AudioClip UIButtonSelected { get; private set; }
    [field: SerializeField] public AudioClip UIButtonPressed { get; private set; }
    [field: SerializeField] public AudioClip UIButtonCanceled { get; private set; }
    [field: SerializeField] public AudioClip UIButtonNavigate { get; private set; }
    [field: SerializeField] public AudioClip UIPopPanel { get; private set; }
    [field: SerializeField] public AudioClip UICountDown { get; private set; }
    [field: SerializeField] public AudioClip UIWinner { get; private set; }
    
    //Musics
    [field: SerializeField] public AudioClip MenuMusic { get; private set; }
    [field: SerializeField] public AudioClip GameMusic { get; private set; }
    
    //Generic
    [field: SerializeField] public List<AudioClip> SunCollected { get; private set; }
    [field: SerializeField] public AudioClip ScientistDialogue { get; private set; }
    // Flowers
    [field: SerializeField] public List<AudioClip> FlowersVoices { get; private set; }
    [field: SerializeField] public AudioClip FlowersRevive { get; private set; }
    [field: SerializeField] public AudioClip FlowersRun { get; private set; }
    [field: SerializeField] public  List<AudioClip> FlowersPlanted { get; private set; }
    [field: SerializeField] public  List<AudioClip> FlowersDeath { get; private set; }
    [field: SerializeField] public AudioClip RoseTrapDestroy { get; private set; }
    
    //Turtle 
    [field: SerializeField] public AudioClip TurtleExplosion { get; private set; }
    [field: SerializeField] public AudioClip TurtleVoice { get; private set; }
    [field: SerializeField] public AudioClip TurtleDialogue { get; private set; }
    [field: SerializeField] public AudioClip TurtleBite { get; private set; }
    [field: SerializeField] public AudioClip TurtleReactor { get; private set; }
    [field: SerializeField] public AudioClip TurtleReactorNitro { get; private set; }
    [field: SerializeField] public AudioClip TurtleEndNitro { get; private set; }
    [field: SerializeField] public AudioClip TurtleStartTurn { get; private set; }
    [field: SerializeField] public AudioClip TurtleTurnAround { get; private set; }
    [field: SerializeField] public AudioClip TurtleDashLvl1 { get; private set; }
    [field: SerializeField] public AudioClip TurtleDashLvl2 { get; private set; }
    [field: SerializeField] public AudioClip TurtleDash { get; private set; }
    [field: SerializeField] public AudioClip TurtleScan { get; private set; }
    [field: SerializeField] public AudioClip TurtleScanAlert { get; private set; }
    [field: SerializeField] public AudioClip TurtleSpawnTrap { get; private set; }
    [field: SerializeField] public AudioClip TurtleTrapActivated { get; private set; }
    [field: SerializeField] public AudioClip TurtleBatteryWarning { get; private set; }
    
    //QTE
    [field: SerializeField] public AudioClip QTEKey { get; private set; }
    [field: SerializeField] public AudioClip QTEFailed { get; private set; }
    [field: SerializeField] public AudioClip QTESuccess { get; private set; }
    
    
}