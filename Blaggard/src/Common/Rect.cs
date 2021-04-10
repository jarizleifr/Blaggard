using static SDL2.SDL;

namespace Blaggard {
  public struct Rect {
    public readonly int x, y, width, height;

    public Rect(int x, int y, int width, int height) {
      this.x = x;
      this.y = y;
      this.width = width;
      this.height = height;
    }

    public int x1 => x;
    public int y1 => y;
    public int x2 => x + width;
    public int y2 => y + height;

    public Rect Expand(int a) => new(x - a, y - a, width + a * 2, height + a * 2);
    public Rect Contract(int a) => new(x + a, y + a, width - a * 2, height - a * 2);

    public bool IsWithinBounds(int x, int y) => (x < x2 && y < y2) && (x >= x1 && y >= y1);

    public bool OutOfBounds(int x, int y, int width, int height)
        => x1 < x || y1 < y || x2 >= width || y2 >= height;


    public bool IsOverlapping(Rect other) => (x2 > other.x1 && x1 < other.x2 && y2 > other.y1 && y1 < other.y2);

    internal SDL_Rect ConvertToSDLRect() => new() { x = x, y = y, w = width, h = height };
  }
}
