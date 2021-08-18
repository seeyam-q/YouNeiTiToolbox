using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE.ExternalAssetLoading
{
    public static class UnityPathHelper
    {
        public static string DecodeUnityPlatformSpecificFolder (string path)
        {
            path = path.Replace("[StreamingAssetsPath]", Application.streamingAssetsPath);
            path = path.Replace("[PersistentDataPath]", Application.persistentDataPath);
            return path;
        }

        public static Dictionary<string, string> GetParameterDictionaryFromUriQuery(string query)
        {
            //e.g. "job_id=20&insert_key=266b270f69"
            string[] splitQuery = query.Split('&');
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            for (int i = 0; i < splitQuery.Length; i++)
            {
                string[] parameter = splitQuery[i].Split('=');
                if (parameter.Length == 2)
                {
                    parameters.Add(parameter[0], parameter[1]);
                }
            }
            return parameters;
        }
    }
}
