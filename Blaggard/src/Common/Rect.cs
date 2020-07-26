using SDL2;

namespace Blaggard.Common
{
    public struct Rect
    {
        public readonly int x, y, width, height;

        public Rect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public int x1 => x;
        public int y1 => y;
        public int x2 => x + width;
        public int y2 => y + height;

        public Rect Expand(int a) => new Rect(x - a, y - a, width + a * 2, height + a * 2);
        public Rect Contract(int a) => new Rect(x + a, y + a, width - a * 2, height - a * 2);

        public bool IsWithinBounds(int x, int y) => (x < x2 && y < y2) && (x >= x1 && y >= y1);

        public bool IsOverlapping(Rect other) => (x2 > other.x1 && x1 < other.x2 && y2 > other.y1 && y1 < other.y2);

        internal SDL.SDL_Rect ConvertToSDLRect() => new SDL.SDL_Rect() { x = x, y = y, w = width, h = height };
    }
}