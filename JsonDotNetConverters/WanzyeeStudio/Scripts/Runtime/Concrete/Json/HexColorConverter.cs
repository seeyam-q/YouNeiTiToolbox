using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Converters
{
    public class HexColorConverter : JsonConverter
    {
        public override bool CanWrite { get { return true; } }
        public override bool CanRead { get { return true; } }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Color);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Color color = Color.white;
            string jsonText = (string)reader.Value;

            ColorUtility.TryParseHtmlString(jsonText, out color);

            return color;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken jToken = JToken.FromObject((ColorUtility.ToHtmlStringRGBA((Color)value)));
            jToken.WriteTo(writer);
        }
    }
}