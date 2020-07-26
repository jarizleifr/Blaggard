using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blaggard.Common
{
    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            new JArray(value.r, value.g, value.b).WriteTo(writer);
        }
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                var arr = (JArray)token;
                return new Color((byte)arr[0], (byte)arr[1], (byte)arr[2]);
            }
            else if (token.Type == JTokenType.Integer)
            {
                var value = (int)token;
                return new Color((byte)(value >> 16), (byte)(value >> 8 & 0xff), (byte)(value & 0xff));
            }
            throw new Exception("Couldn't convert");
        }
    }

    [JsonConverter(typeof(ColorConverter))]
    public struct Color
    {
        public readonly byte r;
        public readonly byte g;
        public readonly byte b;

        public Color(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public static Color white = new Color(255, 255, 255);
        public static Color black = new Color(0, 0, 0);

        public static bool operator ==(Color c1, Color c2) => c1.Equals(c2);
        public static bool operator !=(Color c1, Color c2) => !c1.Equals(c2);

        public override bool Equals(object obj)
        {
            if (!(obj is Color c)) return false;

            return this.AsInt() == c.AsInt();
        }

        public override int GetHashCode() => AsInt();

        private int AsInt() => 0xffff * r + 0xff * g + b;
    }
}