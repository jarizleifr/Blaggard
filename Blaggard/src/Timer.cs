using static SDL2.SDL;

namespace Blaggard {
  public interface ITimer {
    int FPS { get; }
    int MSPF { get; }
    float DeltaTime { get; }
  }

  public class Timer : ITimer {
    public int FPS { get; private set; }
    public int MSPF { get; private set; }
    public float DeltaTime => MSPF / 1000f;

    private uint ticks;
    private int msPassed;
    private int framesPassed;

    public Timer() => ticks = SDL_GetTicks();

    public void Measure() {
      framesPassed++;

      var now = SDL_GetTicks();
      MSPF = (int)(now - ticks);
      msPassed += MSPF;

      ticks = now;

      if (msPassed > 1000) {
        FPS = framesPassed;
        msPassed = 0;
        framesPassed = 0;
      }
    }
  }
}
