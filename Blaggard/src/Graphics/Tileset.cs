using System;
using static SDL2.SDL;

namespace Blaggard {
  public readonly struct Tileset : IDisposable {
    public IntPtr Texture { get; init; }
    public int Count { get; init; }
    public int TileWidth { get; init; }
    public int TileHeight { get; init; }

    public SDL_Rect GetRect(int tileIndex) => new() {
      x = tileIndex % 16 * TileWidth,
      y = tileIndex / 16 * TileHeight,
      w = TileWidth,
      h = TileHeight,
    };

    public void Dispose() {
      SDL_DestroyTexture(Texture);
      GC.SuppressFinalize(this);
    }
  }
}
