using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FortySevenE.Bootstrapper
{
    public interface IBootstrapRawTextParser
    {
        bool TryGetSettingDictionary(string rawText, ref Dictionary<string, object> settingDictionary);
        bool TryParseRawText(string rawText, Type type, out object referenceValue);
        string SerializeDictionary(Dictionary<string, object> value);
    }
}
