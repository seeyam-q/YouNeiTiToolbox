//http://wiki.unity3d.com/index.php/Singleton

using UnityEngine;

namespace FortySevenE
{
    /// <summary>
    /// Inherit from this base class to create a singleton.
    /// e.g. public class MyClassName : Singleton<MyClassName> {}
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Check to see if we're about to be destroyed.
        //private static bool _shuttingDown = false;
        private static object _lock = new object();
        private static T _instance;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T X
        {
            get
            {
                //if (_shuttingDown)
                //{
                //    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                //        "' already destroyed. Returning null.");
                //    return null;
                //}

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // Search for existing instance.
                        _instance = (T)FindObjectOfType(typeof(T));

                        // Create new instance if one doesn't already exist.
                        if (_instance == null)
                        {
                            // Need to create a new GameObject to attach the singleton to.
                            var singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        }

                        if (_instance != null)
                        {
                            if (Application.isPlaying)
                            {
                                // Make instance persistent.
                                DontDestroyOnLoad(_instance);
                            }
                        }
                    }

                    return _instance;
                }
            }
        }


        //private void OnApplicationQuit()
        //{
        //    if (Application.isPlaying)
        //    {
        //        _shuttingDown = true;
        //    }
        //}


        //private void OnDestroy()
        //{
        //    if (Application.isPlaying)
        //    {
        //        _shuttingDown = true;
        //    }
        //}
    }
}