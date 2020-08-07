using System;
using System.Collections.Generic;
using System.Linq;
using SDL2;
using Blaggard.Common;

namespace Blaggard.Graphics
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public interface IBlittable : IDisposable
    {
        IntPtr Texture { get; }
        Rect RenderRect { get; }
        bool Dirty { get; }

        Color DefaultFore { get; set; }
        Color DefaultBack { get; set; }

        int Width { get; }
        int Height { get; }

        void SetRenderPosition(int screenX, int screenY);

        void ResetColors();
        void Clear();

        void Rect(Rect rect, char ch) => Rect(rect.x, rect.y, rect.width, rect.height, ch, DefaultFore, DefaultBack);
        void Rect(int x, int y, int w, int h, char ch) => Rect(x, y, w, h, ch, DefaultFore, DefaultBack);
        void Rect(int x, int y, int w, int h, char ch, Color fore, Color back)
        {
            for (int iy = y; iy < y + h; iy++)
            {
                for (int ix = x; ix < x + w; ix++)
                {
                    SetCell(ix, iy, ch, fore, back);
                }
            }
        }

        void LineVert(int x, int y, int l, char ch) => LineVert(x, y, l, ch, DefaultFore, DefaultBack);
        void LineVert(int x, int y, int l, char ch, Color fore) => LineVert(x, y, l, ch, fore, null);
        void LineVert(int x, int y, int l, char ch, Color fore, Color? back)
        {
            for (int i = 0; i < l; i++)
            {
                SetCell(x, y + i, ch, fore, back);
            }
        }

        void LineHoriz(int x, int y, int l, char ch) => LineHoriz(x, y, l, ch, DefaultFore, DefaultBack);
        void LineHoriz(int x, int y, int l, char ch, Color fore) => LineHoriz(x, y, l, ch, fore, null);
        void LineHoriz(int x, int y, int l, char ch, Color fore, Color? back)
        {
            for (int i = 0; i < l; i++)
            {
                SetCell(x + i, y, ch, fore, back);
            }
        }

        void PrintFrame(bool clear = false) => PrintFrame(0, 0, Width, Height, clear);
        void PrintFrame(Rect rect, bool clear = false) => PrintFrame(rect.x, rect.y, rect.width, rect.height, clear);
        void PrintFrame(int x, int y, int w, int h, bool clear = false)
        {
            LineHoriz(x, y, w - 1, '═', DefaultFore, DefaultBack);
            LineHoriz(x, y + h - 1, w - 1, '═', DefaultFore, DefaultBack);

            LineVert(x, y, h - 1, '║', DefaultFore, DefaultBack);
            LineVert(x + w - 1, y, h - 1, '║', DefaultFore, DefaultBack);

            SetCell(x, y, '╔', DefaultFore, DefaultBack);
            SetCell(x + w - 1, y, '╗', DefaultFore, DefaultBack);
            SetCell(x, y + h - 1, '╚', DefaultFore, DefaultBack);
            SetCell(x + w - 1, y + h - 1, '╝', DefaultFore, DefaultBack);

            if (clear)
            {
                Rect(x + 1, y + 1, w - 2, h - 2, ' ', DefaultFore, DefaultBack);
            }
        }

        void Print(int x, int y, string str, Color fore) => Print(x, y, str, fore, null, TextAlignment.Left);
        void Print(int x, int y, string str, Color fore, TextAlignment alignment) => Print(x, y, str, fore, null, alignment);
        void Print(int x, int y, string str, Color fore, Color back, TextAlignment alignment) => Print(x, y, str, fore, back, alignment);
        void Print(int x, int y, string str, Color fore, Color? back, TextAlignment alignment)
        {
            int offset = alignment switch
            {
                TextAlignment.Center => -str.Length / 2,
                TextAlignment.Right => -str.Length,
                _ => 0
            };
            for (int i = 0; i < str.Length; i++)
            {
                SetCell(x + i + offset, y, str[i], fore, back);
            }
        }

        void PrintColoredString(int x, int y, ColoredString str) => PrintColoredString(x, y, str, null, TextAlignment.Left);
        void PrintColoredString(int x, int y, ColoredString str, TextAlignment alignment) => PrintColoredString(x, y, str, null, alignment);
        void PrintColoredString(int x, int y, ColoredString str, Color? back, TextAlignment alignment)
        {
            int offset = alignment switch
            {
                TextAlignment.Center => -str.Length / 2,
                TextAlignment.Right => -str.Length,
                _ => 0
            };
            for (int i = 0; i < str.Length; i++)
            {
                SetCell(x + i + offset, y, str[i].ch, str[i].color ?? DefaultFore, back);
            }
        }

        void PutChar(int x, int y, char ch) => SetCell(x, y, ch, DefaultFore, DefaultBack);
        void PutChar(int x, int y, char ch, Color fore) => SetCell(x, y, ch, fore, null);
        void PutChar(int x, int y, char ch, Color fore, Color back) => SetCell(x, y, ch, fore, back);

        void SetChar(int x, int y, char ch) => SetCell(x, y, ch, null, null);

        void SetCharForeground(int x, int y, Color fore) => SetCell(x, y, null, fore, null);
        void SetCharBackground(int x, int y, Color back) => SetCell(x, y, null, null, back);
        void SetCell(int x, int y, char? ch, Color? fore, Color? back);

        void DrawSprite(Sprite sprite)
        {
            throw new Exception("Current canvas doesn't support sprites!");
        }

        void Render();
    }

    public abstract class BaseCanvas : IDisposable
    {
        protected Display display;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Color DefaultFore { get; set; }
        public Color DefaultBack { get; set; }

        public void ResetColors()
        {
            DefaultFore = Color.white;
            DefaultBack = Color.black;
        }

        public Rect RenderRect { get; protected set; }

        public bool Dirty { get; protected set; }

        public IntPtr Texture { get; protected set; }

        public BaseCanvas(Display display, int width, int height)
        {
            this.display = display;
            Width = width;
            Height = height;

            Texture = display.CreateTexture(Width * display.CellWidth, Height * display.CellHeight);
        }

        public void Dispose() => display.FreeTexture(Texture);
    }

    public class SpriteCanvas : BaseCanvas, IBlittable
    {
        private List<Sprite> sprites = new List<Sprite>();

        public SpriteCanvas(Display display, int width, int height) : base(display, width, height) { }

        public void Clear()
        {
            sprites.Clear();
            Dirty = true;
        }
        public void SetRenderPosition(int screenX, int screenY)
        {
            RenderRect = new Rect(screenX * display.CellWidth, screenY * display.CellHeight, Width * display.CellWidth, Height * display.CellHeight);
        }

        public void SetCell(int x, int y, char? ch, Color? fore, Color? back) =>
            throw new Exception("Cannot draw cells on SpriteCanvas!");

        public void DrawSprite(Sprite sprite)
        {
            sprites.Add(sprite);
            Dirty = true;
        }

        public void Render()
        {
            if (Dirty)
            {
                display.SetRenderTarget(Texture);
                display.ClearTexture();

                if (sprites.Count > 0)
                {
                    foreach (var sprite in sprites.OrderBy(s => s.zIndex))
                    {
                        display.DrawSprite(sprite);
                    }
                }

                Dirty = false;
            }
        }
    }

    public class SparseCanvas : BaseCanvas, IBlittable
    {
        private Dictionary<Vec2, Cell> cells;

        public SparseCanvas(Display display, int width, int height) : base(display, width, height)
        {
            cells = new Dictionary<Vec2, Cell>();
            SDL.SDL_SetTextureBlendMode(Texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            ResetColors();
            Dirty = true;
        }

        public void SetRenderPosition(int screenX, int screenY)
        {
            RenderRect = new Rect(screenX * display.CellWidth, screenY * display.CellHeight, Width * display.CellWidth, Height * display.CellHeight);
        }

        public void Clear() => cells.Clear();

        public void SetCell(int x, int y, char? ch, Color? fore, Color? back)
        {
            if (Util.IsWithinBounds(x, y, Width, Height))
            {
                var pos = new Vec2(x, y);
                cells[pos] = (cells.TryGetValue(pos, out var cell))
                    ? new Cell(ch ?? cell.ch, fore ?? cell.fore, back ?? cell.back)
                    : new Cell(ch ?? ' ', fore ?? DefaultFore, back ?? DefaultBack);
                Dirty = true;
            }
        }

        ///<summary>Render canvas cells to owned texture.</summary>
        public void Render()
        {
            if (Dirty)
            {
                display.SetRenderTarget(Texture);
                display.ClearTexture();
                foreach ((var pos, var cell) in cells)
                {
                    display.DrawCell(pos.x, pos.y, cell.ch, cell.fore, cell.back);
                }
                Dirty = false;
            }
        }
    }

    public class Canvas : BaseCanvas, IBlittable
    {
        private Cell[] cells;
        private List<Sprite> sprites = new List<Sprite>();

        public Canvas(Display display, int width, int height) : base(display, width, height)
        {
            ResetColors();
            Clear();
            Dirty = true;
        }

        public void SetRenderPosition(int screenX, int screenY)
        {
            RenderRect = new Rect(screenX * display.CellWidth, screenY * display.CellHeight, Width * display.CellWidth, Height * display.CellHeight);
        }

        public void Clear()
        {
            cells = Enumerable.Repeat(new Cell(' ', DefaultFore, DefaultBack), Width * Height).ToArray();
            sprites.Clear();
            Dirty = true;
        }

        public void SetCell(int x, int y, char? ch, Color? fore, Color? back)
        {
            if (Util.IsWithinBounds(x, y, Width, Height))
            {
                var i = Util.IndexFromXY(x, y, Width);
                var cell = new Cell(ch ?? cells[i].ch, fore ?? cells[i].fore, back ?? cells[i].back);
                if (cells[i] != cell)
                {
                    cells[i] = cell;
                    Dirty = true;
                }
            }
        }

        public void DrawSprite(Sprite sprite)
        {
            sprites.Add(sprite);
            Dirty = true;
        }

        ///<summary>Render canvas cells to owned texture.</summary>
        public void Render()
        {
            if (Dirty)
            {
                display.SetRenderTarget(Texture);
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        var c = cells[Util.IndexFromXY(x, y, Width)];
                        display.DrawCell(x, y, c.ch, c.fore, c.back);
                    }
                }

                if (sprites.Count > 0)
                {
                    foreach (var sprite in sprites.OrderBy(s => s.zIndex))
                    {
                        display.DrawSprite(sprite);
                    }
                }
                Dirty = false;
            }
        }
    }
}
