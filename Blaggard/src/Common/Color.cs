using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blaggard {
  public class ColorConverter : JsonConverter<Color> {
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {
      new JArray(value.r, value.g, value.b).WriteTo(writer);
    }
    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {
      var token = JToken.Load(reader);
      if (token.Type == JTokenType.Array) {
        var arr = (JArray)token;
        return new Color((byte)arr[0], (byte)arr[1], (byte)arr[2]);
      } else if (token.Type == JTokenType.Integer) {
        return new Color((int)token);
      }
      throw new Exception("Couldn't convert");
    }
  }

  [JsonConverter(typeof(ColorConverter))]
  public struct Color {
    public readonly byte r, g, b;

    public Color(byte r, byte g, byte b) =>
      (this.r, this.g, this.b) = (r, g, b);

    public Color(int val) =>
      (r, g, b) = ((byte)(val >> 16), (byte)(val >> 8 & 0xff), (byte)(val & 0xff));

    public static readonly Color white = new(255, 255, 255);
    public static readonly Color black = new(0, 0, 0);

    public static bool operator ==(Color c1, Color c2) => c1.Equals(c2);
    public static bool operator !=(Color c1, Color c2) => !c1.Equals(c2);

    public override bool Equals(object obj) => obj is Color c && AsInt() == c.AsInt();

    public override int GetHashCode() => AsInt();

    private int AsInt() => 0xffff * r + 0xff * g + b;
  }
}
