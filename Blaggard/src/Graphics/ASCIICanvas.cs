using System;
using static SDL2.SDL;

namespace Blaggard {
  public struct Cell {
    public int ch;
    public byte fg;
    public byte bg;
  }

  public class ASCIICanvas : IDisposable {
    private readonly IntPtr renderer;

    public int Width { get; init; }
    public int Height { get; init; }
    public int TileWidth => tileset.TileWidth;
    public int TileHeight => tileset.TileHeight;

    private readonly Tileset tileset;
    private readonly Palette palette;

    private readonly Cell[] cells;
    private readonly IntPtr background;

    public byte DefaultFore { get; set; }
    public byte DefaultBack { get; set; }

    public ASCIICanvas(Display display, int width, int height, Tileset tileset, Palette palette) {
      renderer = display.Renderer;

      Width = width;
      Height = height;
      this.tileset = tileset;
      this.palette = palette;

      cells = new Cell[width * height];
      background = display.CreateTexture(width, height);
      display.RegisterCanvas(this);
    }

    public void Dispose() {
      SDL_DestroyTexture(background);
      GC.SuppressFinalize(this);
    }

    public void ResetColors() {
      DefaultFore = 1;
      DefaultBack = 0;
    }

    public void Clear() {
      for (int i = 0; i < cells.Length; i++) {
        ref var cell = ref cells[i];
        cell.ch = 0;
        cell.bg = 0;
        cell.fg = 0;
      }
    }

    public void Clear(byte bg) {
      for (int i = 0; i < cells.Length; i++) {
        ref var cell = ref cells[i];
        cell.ch = 0;
        cell.bg = bg;
        cell.fg = 0;
      }
    }

    public void PrintColoredTextSpan(int x, int y, ColoredTextSpan span) =>
        PrintColoredTextSpan(x, y, span, DefaultBack, Align.Left);

    public void PrintColoredTextSpan(int x, int y, ColoredTextSpan span, Align alignment) =>
        PrintColoredTextSpan(x, y, span, DefaultBack, alignment);

    public void PrintColoredTextSpan(int x, int y, ColoredTextSpan span, byte bg, Align alignment) {
      int offset = 0;
      foreach (var str in span) {
        PrintColoredString(x + offset, y, str, bg, alignment);
        offset += str.Length;
      }
    }

    public void PrintColoredString(int x, int y, ColoredString str) =>
      PrintColoredString(x, y, str, DefaultBack, Align.Left);

    public void PrintColoredString(int x, int y, ColoredString str, Align alignment) =>
      PrintColoredString(x, y, str, DefaultBack, alignment);

    public void PrintColoredString(int x, int y, ColoredString str, byte bg, Align alignment) =>
        Print(x, y, str.Text, str.Color, bg, alignment);

    public void Print(int x, int y, string str) =>
      Print(x, y, str, DefaultFore, DefaultBack, Align.Left);

    public void Print(int x, int y, string str, byte fg) =>
      Print(x, y, str, fg, DefaultBack, Align.Left);

    public void Print(int x, int y, string str, byte fg, Align alignment) =>
      Print(x, y, str, fg, DefaultBack, alignment);

    public void Print(int x, int y, string str, byte fg, byte bg, Align alignment) {
      int offset = alignment switch {
        Align.Center => -str.Length / 2,
        Align.Right => -str.Length,
        _ => 0
      };
      for (int i = 0; i < str.Length; i++) {
        ref var cell = ref cells[y * Width + x + i + offset];
        cell.ch = str[i];
        cell.fg = fg;
        cell.bg = bg;
      }
    }

    public void Rect(Rect rect, int ch) =>
      Rect(rect.x, rect.y, rect.width, rect.height, ch, DefaultFore, DefaultBack);

    public void Rect(int x, int y, int w, int h, int ch) =>
      Rect(x, y, w, h, ch, DefaultFore, DefaultBack);

    public void Rect(int x, int y, int w, int h, int ch, byte fg, byte bg) {
      for (int i = y; i < y + h; i++) {
        for (int j = x; j < x + w; j++) {
          ref var cell = ref cells[i * Width + x + j];
          cell.ch = ch;
          cell.fg = fg;
          cell.bg = bg;
        }
      }
    }

    public void RectBack(int x, int y, int w, int h, byte bg) {
      for (int i = y; i < y + h; i++) {
        for (int j = x; j < x + w; j++) {
          ref var cell = ref cells[i * Width + j];
          cell.bg = bg;
        }
      }
    }

    public void PrintFrame(bool clear = false) => PrintFrame(0, 0, Width, Height, clear);
    public void PrintFrame(Rect rect, bool clear = false) => PrintFrame(rect.x, rect.y, rect.width, rect.height, clear);
    public void PrintFrame(int x, int y, int w, int h, bool clear = false) {
      for (int yy = 0; yy < h; yy++) {
        if (yy == 0) {
          SetCell(x, y, (int)CP437.BoxDrawingsDoubleDownAndRight, DefaultFore, DefaultBack);
          LineHoriz(x + 1, y, w - 2, (int)CP437.BoxDrawingsDoubleHorizontal, DefaultFore, DefaultBack);
          SetCell(x + w - 1, y, (int)CP437.BoxDrawingsDoubleDownAndLeft, DefaultFore, DefaultBack);
        } else if (yy == h - 1) {
          SetCell(x, y + h - 1, (int)CP437.BoxDrawingsDoubleUpAndRight, DefaultFore, DefaultBack);
          LineHoriz(x + 1, y + h - 1, w - 2, (int)CP437.BoxDrawingsDoubleHorizontal, DefaultFore, DefaultBack);
          SetCell(x + w - 1, y + h - 1, (int)CP437.BoxDrawingsDoubleUpAndLeft, DefaultFore, DefaultBack);
        } else {
          SetCell(x, y + yy, (int)CP437.BoxDrawingsDoubleVertical, DefaultFore, DefaultBack);
          if (clear) {
            LineHoriz(x + 1, y + yy, w - 2, ' ', DefaultFore, DefaultBack);
          }
          SetCell(x + w - 1, y + yy, (int)CP437.BoxDrawingsDoubleVertical, DefaultFore, DefaultBack);
        }
      }
    }

    public void LineVert(int x, int y, int h, int ch) =>
      LineVert(x, y, h, ch, DefaultFore, DefaultBack);

    public void LineVert(int x, int y, int h, int ch, byte fg, byte bg) {
      for (int i = 0; i < h; i++) {
        ref var cell = ref cells[(y + i) * Width + x];
        cell.ch = ch;
        cell.fg = fg;
        cell.bg = bg;
      }
    }

    public void LineHoriz(int x, int y, int w, int ch) =>
      LineHoriz(x, y, w, ch, DefaultFore, DefaultBack);

    public void LineHoriz(int x, int y, int w, int ch, byte fg) =>
      LineHoriz(x, y, w, ch, fg, DefaultBack);

    public void LineHoriz(int x, int y, int w, int ch, byte fg, byte bg) {
      for (int i = 0; i < w; i++) {
        ref var cell = ref cells[y * Width + x + i];
        cell.ch = ch;
        cell.fg = fg;
        cell.bg = bg;
      }
    }

    public void PutChar(int x, int y, int ch) =>
      PutChar(x, y, ch, DefaultFore);

    public void PutChar(int x, int y, int ch, byte fg) {
      ref var cell = ref cells[y * Width + x];
      cell.ch = ch;
      cell.fg = fg;
    }

    public void SetCellForeground(int x, int y, byte fg) {
      ref var cell = ref cells[y * Width + x];
      cell.fg = fg;
    }

    public void SetCellBackground(int x, int y, byte bg) {
      ref var cell = ref cells[y * Width + x];
      cell.bg = bg;
    }

    public void SetCell(int x, int y, int ch) => SetCell(x, y, ch, DefaultFore, DefaultBack);
    public void SetCell(int x, int y, int ch, byte fg) => SetCell(x, y, ch, fg, DefaultBack);
    public void SetCell(int x, int y, int ch, byte fg, byte bg) {
      ref var cell = ref cells[y * Width + x];
      cell.ch = ch;
      cell.fg = fg;
      cell.bg = bg;
    }

    public void Render() => Render(0, 0);

    public void Render(int x, int y) {
      SDL_SetRenderTarget(renderer, background);
      for (int i = 0; i < Width * Height; i++) {
        var cell = cells[i];
        var (r, g, b) = palette[cell.bg];
        SDL_SetRenderDrawColor(renderer, r, g, b, 255);
        SDL_RenderDrawPoint(renderer, i % Width, i / Width);
      }
      SDL_SetRenderTarget(renderer, IntPtr.Zero);

      SDL_Rect dst;
      dst.x = x * tileset.TileWidth;
      dst.y = y * tileset.TileHeight;
      dst.w = Width * tileset.TileWidth;
      dst.h = Height * tileset.TileHeight;

      SDL_RenderCopy(renderer, background, IntPtr.Zero, ref dst);

      for (int i = 0; i < cells.Length; i++) {
        ref var cell = ref cells[i];

        var src = tileset.GetRect(cell.ch);

        dst.x = (i % Width + x) * tileset.TileWidth;
        dst.y = (i / Width + y) * tileset.TileHeight;
        dst.w = tileset.TileWidth;
        dst.h = tileset.TileHeight;

        SDL_SetTextureBlendMode(tileset.Texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
        var (r, g, b) = palette[cell.fg];
        SDL_SetTextureColorMod(tileset.Texture, r, g, b);
        SDL_RenderCopy(renderer, tileset.Texture, ref src, ref dst);
      }
    }
  }
}
