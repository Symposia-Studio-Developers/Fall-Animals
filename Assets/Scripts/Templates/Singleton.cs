using UnityEngine;

namespace Fall_Friends.Templates
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = GetComponent<T>();
            }
        }
    }
}