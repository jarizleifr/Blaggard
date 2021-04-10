using System;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace Blaggard {
  public abstract class Context : IDisposable {
    private uint ticks;
    public Context() {
      SDL_Init(SDL_INIT_VIDEO);
      IMG_Init(IMG_InitFlags.IMG_INIT_PNG);

      ticks = SDL_GetTicks();
    }

    public float GetDeltaTime() {
      var now = SDL_GetTicks();
      var deltaTime = (now - ticks) / 1000f;
      ticks = now;
      return deltaTime;
    }

    public void WaitNextFrame(float time) {
      SDL_Delay((uint)(time * 1000f));
    }

    public void Dispose() {
      SDL_Quit();
      Cleanup();
    }

    public abstract void Cleanup();
  }
}
