using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Blaggard {
  public static class Util {
    public static IEnumerable<int> GetNeighbors(int index, int width) {
      yield return SouthWest(index, width);
      yield return South(index, width);
      yield return SouthEast(index, width);
      yield return West(index);
      yield return East(index);
      yield return NorthWest(index, width);
      yield return North(index, width);
      yield return NorthEast(index, width);
    }

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
