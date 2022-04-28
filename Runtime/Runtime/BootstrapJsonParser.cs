using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FortySevenE.Bootstrapper
{ 
    public class BootstrapJsonParser: IBootstrapRawTextParser
    {

        public bool TryGetSettingDictionary(string rawText, ref Dictionary<string, object> settingDictionary)
        {
            try
            {
                settingDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawText);
                return true;
            }
            catch
            {
                Debug.LogWarning("Cannot parse bootstrap.");
            }
            return false;
        }

        public bool TryParseRawText(string rawText, Type type, out object referenceValue)
        {
            try
            {
                referenceValue = JsonConvert.DeserializeObject(rawText, type);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            referenceValue = null;
            return false;
        }

        public string SerializeDictionary(Dictionary<string, object> value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented);
        }
    }
}
