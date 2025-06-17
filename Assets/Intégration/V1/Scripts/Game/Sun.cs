using Intégration.V1.Scripts.Game.Characters;
using Michael.Scripts.Manager;
using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class Sun : MonoBehaviour
    {
        private bool _collected = false;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Flower"))
            {
                if (!_collected && !GameManager.Instance.GameFinished)
                {
                    FlowerController flowerController = other.GetComponent<FlowerController>();
                    
                    if (flowerController.Sun < flowerController.maxSun)
                    {
                        AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.SunCollected);
                        if (other.GetComponent<CloverController>())
                        {
                            GameManager.Instance._sunOccupiedSpawns.Remove(gameObject);

                            int rngLuckCost = Random.Range(0, 4);
                            if (rngLuckCost != 1)
                            {
                                other.GetComponent<FlowerController>().AddSun(1);
                            }
                            else
                            {
                                other.GetComponent<FlowerController>().AddSun(3);
                                Debug.Log("chanceux");
                            }

                            _collected = true;
                            Destroy(gameObject);
                        }
                        else
                        {
                            GameManager.Instance._sunOccupiedSpawns.Remove(gameObject);
                            other.GetComponent<FlowerController>().AddSun(1);
                            _collected = true;
                            Destroy(gameObject);
                        }
                    }
                }
            }

            if (other.CompareTag("Turtle"))
            {
                if (!_collected && !GameManager.Instance.GameFinished)
                {
                    AudioManager.Instance.PlayRandomSound(AudioManager.Instance.ClipsIndex.SunCollected);
                    GameManager.Instance._sunOccupiedSpawns.Remove(gameObject);
                    BatteryManager.OnSunCollected.Invoke(15);
                    _collected = true;
                    Destroy(gameObject);
                    ;
                }
            }
        }
    }
}