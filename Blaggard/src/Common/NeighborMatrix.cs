using System;

namespace Blaggard {
  public struct NeighborMatrix {
    private static readonly Vec2[] matrixTransforms = {
      new Vec2(-1,-1),
      new Vec2( 0,-1),
      new Vec2( 1,-1),
      new Vec2( 1, 0),
      new Vec2( 1, 1),
      new Vec2( 0, 1),
      new Vec2(-1, 1),
      new Vec2(-1, 0),
    };

    private readonly byte bits;

    public NeighborMatrix(Vec2 origin, Func<Vec2, bool> callback) {
      bits = 0;
      for (byte i = 0; i < 8; i++) {
        if (callback.Invoke(origin + matrixTransforms[i])) {
          bits |= (byte)(1 << i);
        }
      }
    }

    public bool IsSet(byte check) => check == (byte)(bits ^ (bits & ~check));
  }
}
