using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blaggard {
  public class Vec2Converter : JsonConverter<Vec2> {
    public override void WriteJson(JsonWriter writer, Vec2 value, JsonSerializer serializer) {
      new JArray(value.x, value.y).WriteTo(writer);
    }
    public override Vec2 ReadJson(JsonReader reader, Type objectType, Vec2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
      var arr = (JArray)JToken.Load(reader);
      return new Vec2((int)arr[0], (int)arr[1]);
    }
  }

  [JsonConverter(typeof(Vec2Converter))]
  public readonly struct Vec2 : IEquatable<Vec2> {
    public readonly int x;
    public readonly int y;

    public Vec2(int x, int y) {
      this.x = x;
      this.y = y;
    }

    public static Vec2 Zero => new(0, 0);
    public static Vec2 FromIndex(int index, int width) => new(index % width, index / width);
    public static Vec2 FromDirection(Direction dir) => dir switch {
      Direction.SouthWest => new(-1, 1),
      Direction.South => new(0, 1),
      Direction.SouthEast => new(1, 1),
      Direction.West => new(-1, 0),
      Direction.East => new(1, 0),
      Direction.NorthWest => new(-1, -1),
      Direction.North => new(0, -1),
      Direction.NorthEast => new(1, -1),
      _ => Zero
    };

    public bool IsWithinCircle(Vec2 target, int radius) {
      int dx = Math.Abs(target.x - x);
      int dy = Math.Abs(target.y - y);

      if (dx > radius || dy > radius) {
        return false;
      }

      if (dx + dy <= radius) {
        return true;
      }

      if (dx * dx + dy * dy <= radius * radius) {
        return true;
      } else {
        return false;
      }
    }

    public int ToIndex(int width) => x + y * width;

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.x + b.x, a.y + b.y);

    public static bool operator ==(Vec2 a, Vec2 b) => a.Equals(b);
    public static bool operator !=(Vec2 a, Vec2 b) => !a.Equals(b);

    public bool Equals(Vec2 other) => x == other.x && y == other.y;

    public override int GetHashCode() => HashCode.Combine(x, y);

    public override bool Equals(object obj) => obj is Vec2 v && x == v.x && y == v.y;
  }
}
