using System;
using SDL2;

namespace Blaggard
{
    public abstract class Context : IDisposable
    {
        private uint ticks;
        public Context()
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);

            ticks = SDL.SDL_GetTicks();
        }

        public float GetDeltaTime()
        {
            var now = SDL.SDL_GetTicks();
            var deltaTime = (now - ticks) / 1000f;
            ticks = now;
            return deltaTime;
        }

        public void WaitNextFrame(float time)
        {
            SDL.SDL_Delay((uint)(time * 1000f));
        }

        public void Dispose()
        {
            SDL.SDL_Quit();
            Cleanup();
        }

        public abstract void Cleanup();
    }
}
