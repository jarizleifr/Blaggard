using System.Runtime.CompilerServices;

namespace Blaggard {
  public static class Util {
    public static int[] GetNeighbors(int index, int width) => new int[8] {
      SouthWest(index, width),
      South(index, width),
      SouthEast(index, width),
      West(index),
      East(index),
      NorthWest(index, width),
      North(index, width),
      NorthEast(index, width),
    };

    private static int SouthWest(int i, int w) => i - 1 + w;
    private static int South(int i, int w) => i + w;
    private static int SouthEast(int i, int w) => i + 1 + w;
    private static int West(int i) => i - 1;
    private static int East(int i) => i + 1;
    private static int NorthWest(int i, int w) => i - 1 - w;
    private static int North(int i, int w) => i - w;
    private static int NorthEast(int i, int w) => i + 1 - w;

    public static bool IsWithinBounds(int x, int y, int w, int h) => (uint)x < w && (uint)y < h;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexFromXY(int x, int y, int w) => x + y * w;
  }
}
