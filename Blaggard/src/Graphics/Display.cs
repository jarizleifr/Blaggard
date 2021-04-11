using System;
using System.Collections.Generic;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace Blaggard {
  public enum FrameRateMode {
    Off = -1,
    VSync = 0,
    Limit30 = 30,
    Limit60 = 60,
  }

  public class Display : IDisposable {
    private readonly IntPtr window;
    public IntPtr Renderer { get; private init; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public FrameRateMode FrameRateMode { get; private set; }

    public List<ASCIICanvas> canvases = new();
    public List<Tileset> tilesets = new();

    public Display(
      int windowWidth,
      int windowHeight,
      string title,
      FrameRateMode frameRateMode,
      int viewportWidth,
      int viewportHeight
    ) {
      Width = windowWidth;
      Height = windowHeight;

      window = SDL_CreateWindow(
        title,
        SDL_WINDOWPOS_CENTERED,
        SDL_WINDOWPOS_CENTERED,
        Width,
        Height,
        SDL_WindowFlags.SDL_WINDOW_OPENGL
      );

      FrameRateMode = frameRateMode;

      var flags = SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE
        | SDL_RendererFlags.SDL_RENDERER_ACCELERATED;

      if (frameRateMode == FrameRateMode.VSync) {
        flags |= SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;
      }

      Renderer = SDL_CreateRenderer(window, -1, flags);

      SDL_RenderSetIntegerScale(Renderer, SDL_bool.SDL_TRUE);
      SDL_RenderSetLogicalSize(Renderer, viewportWidth, viewportHeight);
    }

    public void Dispose() {
      foreach (var tileset in tilesets) {
        tileset.Dispose();
      }
      foreach (var canvas in canvases) {
        canvas.Dispose();
      }
      SDL_DestroyRenderer(Renderer);
      SDL_DestroyWindow(window);
      GC.SuppressFinalize(this);
    }

    public IntPtr CreateTexture(int width, int height) => SDL_CreateTexture(
      Renderer,
      SDL_PIXELFORMAT_RGBA8888,
      (int)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
      width,
      height
    );

    public void RegisterCanvas(ASCIICanvas canvas) => canvases.Add(canvas);

    public Tileset LoadTileset(string path, (int w, int h) tile, int count) {
      var tileset = new Tileset {
        Texture = IMG_LoadTexture(Renderer, path),
        TileWidth = tile.w,
        TileHeight = tile.h,
        Count = count,
      };
      tilesets.Add(tileset);
      return tileset;
    }
  }
}
