using System;

namespace Blaggard {
  public class RNG {
    private Random rng = new();

    public void SetSeed(int seed) => rng = new(seed);
    public void ClearSeed() => rng = new();

    public int Int(int max) => rng.Next(max);
    public byte Byte(byte max) => (byte)rng.Next(max);
  }
}
