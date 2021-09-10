using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE
{
    public static class GlobalHashMap
    {
        private static Dictionary<string, int> _animStateHashMap;
        private static Dictionary<string, int> _shaderHashMap;
        public static int GetAnimHash(string key)
        {
            if (_animStateHashMap == null)
            {
                _animStateHashMap = new Dictionary<string, int>();
            }
            if (_animStateHashMap.ContainsKey(key))
            {
                return _animStateHashMap[key];
            }
            else
            {
                var hash = Animator.StringToHash(key);
                _animStateHashMap.Add(key, hash);
                return hash;
            }
        }

        public static int GetShaderHash(string key)
        {
            if (_shaderHashMap == null)
            {
                _shaderHashMap = new Dictionary<string, int>();
            }
            if (_shaderHashMap.ContainsKey(key))
            {
                return _shaderHashMap[key];
            }
            else
            {
                var hash = Shader.PropertyToID(key);
                _shaderHashMap.Add(key, hash);
                return hash;
            }
        }
    }
}