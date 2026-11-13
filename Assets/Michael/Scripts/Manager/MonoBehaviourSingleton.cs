using UnityEngine;

namespace Michael.Scripts.Manager
{
    public class MonoBehaviourSingleton<T> : MonoBehaviour
        where T : Component
    {
        private static T _instance;
        public static T Instance {
            get {
                if (_instance == null) {
                    var objs = FindObjectsOfType (typeof(T)) as T[];
                    if (objs.Length > 0)
                        _instance = objs[0];
                    if (_instance == null) {
                        GameObject obj = new GameObject ();
                        obj.hideFlags = HideFlags.HideAndDontSave;
                        _instance = obj.AddComponent<T> ();
                    }
                }
                return _instance;
            }
        }
    }
}
