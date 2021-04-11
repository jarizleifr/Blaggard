using System.Collections.Generic;
using static SDL2.SDL;

namespace Blaggard {
  public interface IHandler<TCommand> {
    TCommand Peek();
    TCommand Get();
    void SetActive(string[] domains);
  }

  public class Handler<TCommand> : IHandler<TCommand> {
    private Dictionary<string, List<KeyInput<TCommand>>> domains = new();
    private readonly Queue<TCommand> commandQueue = new();

    private string[] activeDomains;

    public void SetDomains(Dictionary<string, List<KeyInput<TCommand>>> domains) =>
      this.domains = domains;

    public void AddDomain(string domain, List<KeyInput<TCommand>> commands) =>
      domains.Add(domain, commands);

    public void Handle() {
      if (activeDomains == null) return;

      // Poll for SDL events
      while (SDL_PollEvent(out var ev) == 1) {
        if (ev.type == SDL_EventType.SDL_KEYDOWN) {
          commandQueue.Enqueue(CreateCommand(ev, activeDomains));
        }
      }
    }

    // Returns the next input in queue without dequeing it.
    public TCommand Peek() => (commandQueue.Count > 0)
      ? commandQueue.Peek()
      : default;

    // Returns the next input in queue and dequeues it.
    public TCommand Get() => (commandQueue.Count > 0)
      ? commandQueue.Dequeue()
      : default;

    public void SetActive(string[] domains) => activeDomains = domains;

    private TCommand CreateCommand(SDL_Event ev, string[] activeDomains) {
      foreach (var domain in activeDomains) {
        foreach (var k in domains[domain]) {
          if (k.IsMatch(ev.key.keysym.sym, ev.key.keysym.mod)) {
            return k.Cmd;
          }
        }
      }
      return default;
    }
  }

  public record KeyInput<TCommand> {
    public TCommand Cmd { get; init; }
    public SDL_Keycode Key { get; init; }
    public bool Ctrl { get; init; }
    public bool Alt { get; init; }
    public bool Shift { get; init; }

    public bool IsMatch(SDL_Keycode keycode, SDL_Keymod mod) => (keycode, mod) switch {
      var _ when keycode != Key => false,
      var _ when (mod & SDL_Keymod.KMOD_CTRL) != 0 != Ctrl => false,
      var _ when (mod & SDL_Keymod.KMOD_ALT) != 0 != Alt => false,
      var _ when (mod & SDL_Keymod.KMOD_SHIFT) != 0 != Shift => false,
      _ => true,
    };
  }
}
