using System;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace Blaggard {
  // TODO: Implement recreating display with new resolution via config
  public class Display : IDisposable {
    private readonly IntPtr window, renderer, fontTexture, glyphTexture;
    private readonly int xOffset, yOffset;
    private readonly int glyphWidth, glyphHeight;

    private readonly int mult;

    private bool fullscreen = false;
    private bool dirty;
    private readonly bool terminalMode = false;

    public readonly int width, height;
    public readonly int cellWidth, cellHeight;

    public Display(int xResolution, int yResolution, int pixelMult, bool terminalMode) {
      Console.WriteLine("Initializing graphics subsystem.");

      this.terminalMode = terminalMode;

      mult = pixelMult;

      glyphWidth = 8;
      glyphHeight = 8;
      cellWidth = glyphWidth * mult;
      cellHeight = glyphHeight * mult;

      width = xResolution;
      height = yResolution;

      xOffset = (xResolution * cellWidth - width * cellWidth) / 2;
      yOffset = (yResolution * cellHeight - height * cellHeight) / 2;

      window = SDL_CreateWindow
      (
          "Waste Drudgers",
          SDL_WINDOWPOS_CENTERED,
          SDL_WINDOWPOS_CENTERED,
          xResolution * cellWidth,
          yResolution * cellHeight,
          SDL_WindowFlags.SDL_WINDOW_OPENGL
      );
      renderer = SDL_CreateRenderer(window, -1, SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

      fontTexture = IMG_LoadTexture(renderer, "assets/font8x8.png");
      glyphTexture = IMG_LoadTexture(renderer, "assets/glyph8x8.png");

      SDL_RenderClear(renderer);
    }

    public void Dispose() {
      Console.WriteLine("Disposing Graphics subsystem.");

      FreeTexture(fontTexture);
      FreeTexture(glyphTexture);

      SDL_DestroyRenderer(renderer);
      SDL_DestroyWindow(window);
    }

    public IntPtr CreateTexture(int pixelsWidth, int pixelsHeight) => SDL_CreateTexture
    (
        renderer,
        SDL_PIXELFORMAT_RGBA8888,
        (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
        pixelsWidth,
        pixelsHeight
    );

    public IntPtr GetRenderer() => renderer;

    public void FreeTexture(IntPtr texture) => SDL_DestroyTexture(texture);

    public void Clear(int x, int y, int width, int height, Color background) {
      SDL_SetRenderTarget(renderer, IntPtr.Zero);

      var dstRect = new SDL_Rect() { x = x * cellWidth, y = y * cellHeight, w = width * cellWidth, h = height * cellHeight };

      SDL_SetRenderDrawColor(renderer, background.r, background.g, background.b, 255);
      SDL_RenderFillRect(renderer, ref dstRect);
      dirty = true;
    }

    public void ToggleFullscreen() {
      var flag = fullscreen ? SDL_WindowFlags.SDL_WINDOW_OPENGL : SDL_WindowFlags.SDL_WINDOW_FULLSCREEN;
      SDL_SetWindowFullscreen(window, (uint)flag);
      fullscreen = !fullscreen;
    }

    public void SetRenderTarget(IntPtr texture) =>
        SDL_SetRenderTarget(renderer, texture);

    public void DrawCell(int x, int y, char ch, Color foreground, Color background) {
      bool glyph = false;
      int character;
      if (ch > 255 && ch < 512) {
        glyph = true;
        character = ch - 256;
      } else {
        character = CP437Utils.FromChar(ch);
      }

      int cy = character / 16;
      int cx = character - cy * 16;

      var srcRect = new SDL_Rect { x = cx * glyphWidth, y = cy * glyphHeight, w = glyphWidth, h = glyphHeight };
      var dstRect = new SDL_Rect { x = (x * cellWidth) + xOffset, y = (y * cellHeight) + yOffset, w = cellWidth, h = cellHeight };

      DrawCellBackground(terminalMode ? Color.black : background, ref dstRect);

      var srcTexture = glyph ? glyphTexture : fontTexture;
      SDL_SetTextureBlendMode(srcTexture, SDL_BlendMode.SDL_BLENDMODE_BLEND);
      SDL_SetTextureColorMod(srcTexture, foreground.r, foreground.g, foreground.b);

      SDL_RenderCopy(renderer, srcTexture, ref srcRect, ref dstRect);
    }

    private void DrawCellBackground(Color background, ref SDL_Rect dstRect) {
      SDL_SetRenderDrawColor(renderer, background.r, background.g, background.b, 255);
      SDL_RenderFillRect(renderer, ref dstRect);
    }

    public void DrawSprite(Sprite sprite) {
      int h = sprite.Texture.height;

      int x = sprite.x;
      int y = sprite.y;

      var srcRect = new SDL_Rect() { x = 0, y = 0, w = 24, h = h };
      var dstRect = new SDL_Rect() { x = x, y = y, w = 24, h = h };

      SDL_RenderCopy(renderer, sprite.Texture.Value, ref srcRect, ref dstRect);
    }

    public void ClearTexture() {
      SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
      SDL_RenderClear(renderer);
    }

    // This is used only for generating tilesets in Waste Drudgers, should probably be it's own library altogether
    public void SaveTextureToFile(IntPtr texture) {
      var target = SDL_GetRenderTarget(renderer);
      SDL_SetRenderTarget(renderer, texture);
      SDL_QueryTexture(texture, out var format, out var _, out var width, out var height);
      var pixels = System.Runtime.InteropServices.Marshal.AllocHGlobal(width * height * SDL_BYTESPERPIXEL(format));
      var rect = new SDL_Rect() { x = 0, y = 0, w = width, h = height };
      SDL_RenderReadPixels(renderer, ref rect, format, pixels, width * SDL_BYTESPERPIXEL(format));
      var surface = SDL_CreateRGBSurfaceWithFormatFrom(pixels, width, height, 24, width * SDL_BYTESPERPIXEL(format), format);
      IMG_SavePNG(surface, "./editor/tileset.png");
      SDL_FreeSurface(surface);
      SDL_SetRenderTarget(renderer, target);
    }

    /// <summary>
    /// Blit an off-screen canvas to the root texture.
    /// </summary>
    public void Blit(IBlittable blittable) {
      var dstRect = blittable.RenderRect.ConvertToSDLRect();
      SDL_SetRenderTarget(renderer, IntPtr.Zero);
      SDL_RenderCopy(renderer, blittable.Texture, IntPtr.Zero, ref dstRect);
      dirty = true;
    }

    /// <summary>
    /// Flush the root canvas to the renderer.
    /// </summary>
    public void Flush() {
      // Flush only if something was actually drawn to texture since last call
      if (dirty) {
        SDL_RenderPresent(renderer);
        dirty = false;
      }
    }
  }
}
