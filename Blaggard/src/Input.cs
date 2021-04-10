using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using static SDL2.SDL;

namespace Blaggard {
  public class Handler<TCommand> {
    private readonly Domains<TCommand> domains;
    private readonly Queue<TCommand> commandQueue;

    public Handler() {
      Console.WriteLine("Initializing input subsystem.");
      // TODO: Input file should be configurable
      domains = new Domains<TCommand>(File.ReadAllText("assets/input.json"));
      commandQueue = new Queue<TCommand>();
    }

    public void Handle(string[] activeDomains) {
      if (activeDomains is null) return;

      // Poll for SDL events
      while (SDL_PollEvent(out var ev) == 1) {
        if (ev.type == SDL_EventType.SDL_KEYDOWN) {
          var command = domains.GetCommand(ev.key.keysym.sym, ev.key.keysym.mod, activeDomains);
          commandQueue.Enqueue(command);
        }
      }
    }

    // Returns the next input in queue without dequeing it.
    public TCommand Peek() => (commandQueue.Count > 0) ? commandQueue.Peek() : default;

    // Returns the next input in queue and dequeues it.
    public TCommand Get() => (commandQueue.Count > 0) ? commandQueue.Dequeue() : default;
  }

  internal class KeyInput<TCommand> {
    public TCommand Cmd { get; set; }
    public SDL_Keycode Key { get; set; }
    public bool Ctrl { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }

    public bool IsMatch(SDL_Keycode keycode, SDL_Keymod mod) {
      if (keycode != Key) return false;
      if ((mod & SDL_Keymod.KMOD_CTRL) != 0 != Ctrl) return false;
      if ((mod & SDL_Keymod.KMOD_ALT) != 0 != Alt) return false;
      if ((mod & SDL_Keymod.KMOD_SHIFT) != 0 != Shift) return false;

      return true;
    }
  }

  internal class Domains<TCommand> {
    private readonly Dictionary<string, List<KeyInput<TCommand>>> domains;

    public Domains(string data) => domains = JsonConvert.DeserializeObject<Dictionary<string, List<KeyInput<TCommand>>>>(data);

    public TCommand GetCommand(SDL_Keycode key, SDL_Keymod mod, string[] activeDomains) {
      foreach (var domain in activeDomains) {
        foreach (var k in domains[domain]) {
          if (k.IsMatch(key, mod)) return k.Cmd;
        }
      }
      return default;
    }
  }
}
