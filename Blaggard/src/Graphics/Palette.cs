using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Blaggard {
  public class Palette {
    private readonly (byte r, byte g, byte b)[] colors = new (byte, byte, byte)[256];
    private readonly Dictionary<string, byte> indices = new();

    [JsonConstructor]
    public Palette(List<int> colors, Dictionary<string, byte> indices) {
      this.colors = colors.Select((c) => (
        (byte)((c >> 16) & 255),
        (byte)((c >> 8) & 255),
        (byte)(c & 255)
      )).ToArray();
      this.indices = indices;
    }

    public int Count => indices.Count;

    public void AddColor(string key, byte r, byte g, byte b) {
      colors[indices.Count] = (r, g, b);
      indices.Add(key, (byte)indices.Count);
    }

    public (byte r, byte g, byte b) this[int index] => colors[index];
    public byte this[string key] => indices[key];
  }
}
