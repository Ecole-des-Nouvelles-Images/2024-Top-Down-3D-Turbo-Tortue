using UnityEngine;

namespace Intégration.V1.Scripts.Game
{
    public class ScannerArea : MonoBehaviour
    {
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Flower")) {
                
                AudioManager.Instance.PlaySound(AudioManager.Instance.ClipsIndex.TurtleScanAlert);
                other.gameObject.layer = LayerMask.NameToLayer("Flower");
                foreach (Transform child in other.transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Flower");
                    foreach (Transform childtochild in child)
                    {
                        childtochild.gameObject.layer = LayerMask.NameToLayer("Flower");
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Flower")) {
                other.gameObject.layer = LayerMask.NameToLayer("Default");
                foreach (Transform child in other.transform)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Default");
                    foreach (Transform childtochild in child)
                    {
                        childtochild.gameObject.layer = LayerMask.NameToLayer("Default");
                    }
                }
            }
        }
    }
}