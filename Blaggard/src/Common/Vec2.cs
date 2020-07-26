using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blaggard.Common
{
    public class Vec2Converter : JsonConverter<Vec2>
    {
        public override void WriteJson(JsonWriter writer, Vec2 value, JsonSerializer serializer)
        {
            new JArray(value.x, value.y).WriteTo(writer);
        }
        public override Vec2 ReadJson(JsonReader reader, Type objectType, Vec2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var arr = (JArray)JToken.Load(reader);
            return new Vec2((int)arr[0], (int)arr[1]);
        }
    }

    [JsonConverter(typeof(Vec2Converter))]
    public struct Vec2
    {
        public readonly int x;
        public readonly int y;

        public Vec2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2 Zero => new Vec2(0, 0);
        public static Vec2 FromIndex(int index, int width) => new Vec2(index % width, index / width);
        public static Vec2 FromDirection(Direction dir) => dir switch
        {
            Direction.SouthWest => new Vec2(-1, 1),
            Direction.South => new Vec2(0, 1),
            Direction.SouthEast => new Vec2(1, 1),
            Direction.West => new Vec2(-1, 0),
            Direction.East => new Vec2(1, 0),
            Direction.NorthWest => new Vec2(-1, -1),
            Direction.North => new Vec2(0, -1),
            Direction.NorthEast => new Vec2(1, -1),
            _ => Zero
        };

        public bool IsWithinCircle(Vec2 target, int radius)
        {
            int dx = Math.Abs(target.x - x);
            int dy = Math.Abs(target.y - y);

            if (dx > radius || dy > radius)
            {
                return false;
            }

            if (dx + dy <= radius)
            {
                return true;
            }

            if (dx * dx + dy * dy <= radius * radius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int ToIndex(int width) => x + y * width;

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.x + b.x, a.y + b.y);
        public static bool operator ==(Vec2 a, Vec2 b) { return a.Equals(b); }
        public static bool operator !=(Vec2 a, Vec2 b) { return !(a == b); }

        public override bool Equals(object obj) => !(obj is Vec2 v) ? false : this.x == v.x && this.y == v.y;
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = (hashCode * 23) + x.GetHashCode();
                hashCode = (hashCode * 23) + y.GetHashCode();
                return hashCode;
            }
        }
    }
}