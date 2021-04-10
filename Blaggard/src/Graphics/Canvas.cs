using System;
using System.Collections.Generic;
using System.Linq;
using static SDL2.SDL;

namespace Blaggard {
  public enum TextAlignment {
    Left,
    Center,
    Right
  }

  public interface IBlittable : IDisposable {
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
    void Rect(int x, int y, int w, int h, char ch, Color fore, Color back) {
      for (int iy = y; iy < y + h; iy++) {
        for (int ix = x; ix < x + w; ix++) {
          SetCell(ix, iy, ch, fore, back);
        }
      }
    }

    void LineVert(int x, int y, int l, char ch) => LineVert(x, y, l, ch, DefaultFore, DefaultBack);
    void LineVert(int x, int y, int l, char ch, Color fore) => LineVert(x, y, l, ch, fore, DefaultBack);
    void LineVert(int x, int y, int l, char ch, Color fore, Color back) {
      for (int i = 0; i < l; i++) {
        SetCell(x, y + i, ch, fore, back);
      }
    }

    void LineHoriz(int x, int y, int l, char ch) => LineHoriz(x, y, l, ch, DefaultFore, DefaultBack);
    void LineHoriz(int x, int y, int l, char ch, Color fore) => LineHoriz(x, y, l, ch, fore, DefaultBack);
    void LineHoriz(int x, int y, int l, char ch, Color fore, Color back) {
      for (int i = 0; i < l; i++) {
        SetCell(x + i, y, ch, fore, back);
      }
    }

    void PrintFrame(bool clear = false) => PrintFrame(0, 0, Width, Height, clear);
    void PrintFrame(Rect rect, bool clear = false) => PrintFrame(rect.x, rect.y, rect.width, rect.height, clear);
    void PrintFrame(int x, int y, int w, int h, bool clear = false) {
      for (int yy = 0; yy < h; yy++) {
        if (yy == 0) {
          SetCell(x, y, '╔', DefaultFore, DefaultBack);
          LineHoriz(x + 1, y, w - 2, '═', DefaultFore, DefaultBack);
          SetCell(x + w - 1, y, '╗', DefaultFore, DefaultBack);
        } else if (yy == h - 1) {
          SetCell(x, y + h - 1, '╚', DefaultFore, DefaultBack);
          LineHoriz(x + 1, y + h - 1, w - 2, '═', DefaultFore, DefaultBack);
          SetCell(x + w - 1, y + h - 1, '╝', DefaultFore, DefaultBack);
        } else {
          SetCell(x, y + yy, '║', DefaultFore, DefaultBack);
          if (clear) {
            LineHoriz(x + 1, y + yy, w - 2, ' ', DefaultFore, DefaultBack);
          }
          SetCell(x + w - 1, y + yy, '║', DefaultFore, DefaultBack);
        }
      }
    }

    void Print(int x, int y, string str) => Print(x, y, str, DefaultFore, DefaultBack, TextAlignment.Left);
    void Print(int x, int y, string str, Color fore) => Print(x, y, str, fore, DefaultBack, TextAlignment.Left);
    void Print(int x, int y, string str, Color fore, TextAlignment alignment) => Print(x, y, str, fore, DefaultBack, alignment);
    void Print(int x, int y, string str, Color fore, Color back, TextAlignment alignment) {
      int offset = alignment switch {
        TextAlignment.Center => -str.Length / 2,
        TextAlignment.Right => -str.Length,
        _ => 0
      };
      for (int i = 0; i < str.Length; i++) {
        SetCell(x + i + offset, y, str[i], fore, back);
      }
    }

    void PrintColoredTextSpan(int x, int y, ColoredTextSpan span) =>
        PrintColoredTextSpan(x, y, span, DefaultBack, TextAlignment.Left);

    void PrintColoredTextSpan(int x, int y, ColoredTextSpan span, TextAlignment alignment) =>
        PrintColoredTextSpan(x, y, span, DefaultBack, alignment);

    void PrintColoredTextSpan(int x, int y, ColoredTextSpan span, Color back, TextAlignment alignment) {
      int offset = 0;
      foreach (var str in span) {
        PrintColoredString(x + offset, y, str, back, alignment);
        offset += str.Length;
      }
    }
    void PrintColoredString(int x, int y, ColoredString str) => PrintColoredString(x, y, str, DefaultBack, TextAlignment.Left);
    void PrintColoredString(int x, int y, ColoredString str, TextAlignment alignment) => PrintColoredString(x, y, str, DefaultBack, alignment);
    void PrintColoredString(int x, int y, ColoredString str, Color back, TextAlignment alignment) =>
        Print(x, y, str.Text, str.Color, back, alignment);

    ///<summary>Puts a character on console, without changing cell.</summary>
    void PutChar(int x, int y, char ch);
    ///<summary>Puts a colored character on console, without changing background color.</summary>
    void PutChar(int x, int y, char ch, Color fore);

    ///<summary>Sets foreground color of cell.</summary>
    void SetCellForeground(int x, int y, Color fore);
    ///<summary>Sets background color fo cell.</summary>
    void SetCellBackground(int x, int y, Color back);

    ///<summary>Sets cell with provided character and default colors.</summary>
    void SetCell(int x, int y, char ch) => SetCell(x, y, ch, DefaultFore, DefaultBack);
    ///<summary>Sets cell with provided character and foreground, and default background color.</summary>
    void SetCell(int x, int y, char ch, Color fore) => SetCell(x, y, ch, fore, DefaultBack);
    ///<summary>Sets cell with provided cell and colors.</summary>
    void SetCell(int x, int y, char ch, Color fore, Color back);

    void DrawSprite(Sprite sprite) =>
        throw new Exception("Current canvas doesn't support sprites!");

    void Render();
  }

  public abstract class BaseCanvas : IDisposable {
    protected Display display;
    protected int width, height;
    protected bool dirty;

    public int Width => width;
    public int Height => height;
    public bool Dirty => dirty;

    public Color DefaultFore { get; set; }
    public Color DefaultBack { get; set; }

    public void ResetColors() {
      DefaultFore = Color.white;
      DefaultBack = Color.black;
    }

    public Rect RenderRect { get; protected set; }
    public IntPtr Texture { get; protected set; }

    public BaseCanvas(Display display, int width, int height) {
      this.display = display;
      this.width = width;
      this.height = height;

      Texture = display.CreateTexture(width * display.cellWidth, height * display.cellHeight);
    }

    public void Dispose() => display.FreeTexture(Texture);
  }

  public class SpriteCanvas : BaseCanvas, IBlittable {
    private readonly List<Sprite> sprites = new();

    public SpriteCanvas(Display display, int width, int height) : base(display, width, height) { }

    public void Clear() {
      sprites.Clear();
      dirty = true;
    }

    public void SetRenderPosition(int screenX, int screenY) =>
        RenderRect = new Rect(screenX * display.cellWidth, screenY * display.cellHeight, width * display.cellWidth, height * display.cellHeight);

    public void PutChar(int x, int y, char ch) =>
        throw new Exception("Cannot draw cells on SpriteCanvas!");

    public void PutChar(int x, int y, char ch, Color fore) =>
        throw new Exception("Cannot draw cells on SpriteCanvas!");

    public void SetCellForeground(int x, int y, Color fore) =>
        throw new Exception("Cannot draw cells on SpriteCanvas!");

    public void SetCellBackground(int x, int y, Color back) =>
        throw new Exception("Cannot draw cells on SpriteCanvas!");

    public void SetCell(int x, int y, char ch, Color fore, Color back) =>
        throw new Exception("Cannot draw cells on SpriteCanvas!");

    public void DrawSprite(Sprite sprite) {
      sprites.Add(sprite);
      dirty = true;
    }

    public void Render() {
      if (dirty) {
        display.SetRenderTarget(Texture);
        display.ClearTexture();

        if (sprites.Count > 0) {
          foreach (var sprite in sprites.OrderBy(s => s.zIndex)) {
            display.DrawSprite(sprite);
          }
        }

        dirty = false;
      }
    }
  }

  public class SparseCanvas : BaseCanvas, IBlittable {
    private readonly Dictionary<Vec2, Cell> cells;

    public SparseCanvas(Display display, int width, int height) : base(display, width, height) {
      cells = new Dictionary<Vec2, Cell>();
      SDL_SetTextureBlendMode(Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
      ResetColors();
      dirty = true;
    }

    public void SetRenderPosition(int screenX, int screenY) {
      RenderRect = new Rect(screenX * display.cellWidth, screenY * display.cellHeight, width * display.cellWidth, height * display.cellHeight);
    }

    public void Clear() => cells.Clear();

    public void PutChar(int x, int y, char ch) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        var pos = new Vec2(x, y);
        var cell = cells[pos];
        cells[pos] = new Cell(ch, cell.fore, cell.back);
        dirty = true;
      }
    }

    public void PutChar(int x, int y, char ch, Color fore) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        var pos = new Vec2(x, y);
        var cell = cells[pos];
        cells[pos] = new Cell(ch, fore, cell.back);
        dirty = true;
      }
    }

    public void SetCellForeground(int x, int y, Color fore) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        var pos = new Vec2(x, y);
        var cell = cells[pos];
        cells[pos] = new Cell(cell.ch, fore, cell.back);
        dirty = true;
      }
    }

    public void SetCellBackground(int x, int y, Color back) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        var pos = new Vec2(x, y);
        var cell = cells[pos];
        cells[pos] = new Cell(cell.ch, cell.fore, back);
        dirty = true;
      }
    }

    public void SetCell(int x, int y, char ch, Color fore, Color back) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        var pos = new Vec2(x, y);
        cells[pos] = new Cell(ch, fore, back);
        dirty = true;
      }
    }

    ///<summary>Render canvas cells to owned texture.</summary>
    public void Render() {
      if (dirty) {
        display.SetRenderTarget(Texture);
        display.ClearTexture();
        foreach ((var pos, var cell) in cells) {
          display.DrawCell(pos.x, pos.y, cell.ch, cell.fore, cell.back);
        }
        dirty = false;
      }
    }
  }

  public class Canvas : BaseCanvas, IBlittable {
    private readonly Cell[] cells;
    private readonly List<Sprite> sprites = new();

    public Canvas(Display display, int width, int height) : base(display, width, height) {
      cells = new Cell[width * height];
      ResetColors();
      Clear();
      dirty = true;
    }

    public void SetRenderPosition(int screenX, int screenY) =>
        RenderRect = new Rect(screenX * display.cellWidth, screenY * display.cellHeight, width * display.cellWidth, height * display.cellHeight);

    public void Clear() {
      for (int i = 0; i < width * height; i++) {
        ref var cell = ref cells[i];
        cell.ch = ' ';
        cell.fore = DefaultFore;
        cell.back = DefaultBack;
        dirty = true;
      }
      sprites.Clear();
      dirty = true;
    }

    public void PutChar(int x, int y, char ch) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        ref var cell = ref cells[Util.IndexFromXY(x, y, width)];
        cell.ch = ch;
        dirty = true;
      }
    }

    public void PutChar(int x, int y, char ch, Color fore) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        ref var cell = ref cells[Util.IndexFromXY(x, y, width)];
        cell.ch = ch;
        cell.fore = fore;
        dirty = true;
      }
    }

    public void SetCellForeground(int x, int y, Color fore) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        ref var cell = ref cells[Util.IndexFromXY(x, y, width)];
        cell.fore = fore;
        dirty = true;
      }
    }

    public void SetCellBackground(int x, int y, Color back) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        ref var cell = ref cells[Util.IndexFromXY(x, y, width)];
        cell.back = back;
        dirty = true;
      }
    }

    public void SetCell(int x, int y, char ch, Color fore, Color back) {
      if (Util.IsWithinBounds(x, y, width, height)) {
        ref var cell = ref cells[Util.IndexFromXY(x, y, width)];
        cell.ch = ch;
        cell.fore = fore;
        cell.back = back;
        dirty = true;
      }
    }

    public void DrawSprite(Sprite sprite) {
      sprites.Add(sprite);
      dirty = true;
    }

    ///<summary>Render canvas cells to owned texture.</summary>
    public void Render() {
      if (dirty) {
        display.SetRenderTarget(Texture);
        for (int y = 0; y < height; y++) {
          for (int x = 0; x < width; x++) {
            var c = cells[Util.IndexFromXY(x, y, width)];
            display.DrawCell(x, y, c.ch, c.fore, c.back);
          }
        }

        if (sprites.Count > 0) {
          foreach (var sprite in sprites.OrderBy(s => s.zIndex)) {
            display.DrawSprite(sprite);
          }
        }
        dirty = false;
      }
    }
  }
}
