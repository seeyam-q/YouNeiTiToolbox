// From by https://hextantstudios.com/unity-singletons/

using System;
using UnityEngine;

namespace FortySevenE
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        //backward compatibility for old singleton naming
        public static T X => Instance;

        private static bool _warnIfNull;

        public static T Instance
        {
            get
            {
                if (_instance == null) _instance = FindObjectOfType<T>();
                if (_warnIfNull && _instance == null) Debug.LogError("Singleton of type : " + typeof(T).Name + " not found on scene");

                return _instance;
            }
        }
        private static T _instance;


        /// <summary>
        /// Use this function to cache instance and destroy duplicate objects.
        /// Also use DontDestroyOnLoad if "persistent" is not set to false
        /// </summary>
        protected void InitializeSingleton(bool persistent = true, bool warnIfNull = false)
        {
            if (_instance == null)
            {
                _instance = (T)Convert.ChangeType(this, typeof(T));
                if (persistent) DontDestroyOnLoad(_instance);
                _warnIfNull = warnIfNull;
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"Another instance of Singleton<{typeof(T).Name}> detected on GO {name}. Destroyed", gameObject);
                Destroy(this);
            }
        }
        
        // Clear the instance field when destroyed.
        protected virtual void OnDestroy() => _instance = null;
    }
}