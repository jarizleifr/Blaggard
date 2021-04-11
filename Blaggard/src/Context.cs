using static SDL2.SDL;

namespace Blaggard {
  public abstract class Context<TCommand> {
    protected readonly Handler<TCommand> handler;
    public IHandler<TCommand> Handler => handler;

    public Display Display { get; protected set; }

    protected readonly Timer timer;
    public ITimer Timer => timer;

    public RNG RNG { get; private set; }

    public bool SkipNextFrame { get; set; }
    public bool Running { get; protected set; }

    public Context() {
      SDL_Init(SDL_INIT_VIDEO);
      SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "0");

      handler = new();
      timer = new();
      RNG = new();

      Initialize();
    }

    public abstract void Initialize();
    public abstract void Tick();

    public void RunMainLoop() {
      Running = true;
      while (Running) {
        SDL_SetRenderDrawColor(Display.Renderer, 0, 0, 0, 255);
        SDL_RenderClear(Display.Renderer);
        handler.Handle();

        Tick();

        if (!SkipNextFrame) {
          SDL_RenderPresent(Display.Renderer);
          timer.Measure();

          if (Display.FrameRateMode > 0) {
            var ticks = 1000u / (uint)Display.FrameRateMode - (uint)timer.MSPF;
            if (ticks > 2000000) {
              ticks = 0;
            }
            SDL_Delay(ticks);
          }
        }
        SkipNextFrame = false;
      }
    }
  }
}
