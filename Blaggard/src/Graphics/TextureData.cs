using System;
using System.Collections.Generic;
using System.IO;
using static SDL2.SDL;
using static SDL2.SDL_image;

namespace Blaggard {
  public class TextureHandle {
    private readonly IntPtr ptr;
    public readonly int width, height;
    public IntPtr Value => ptr;

    public TextureHandle(IntPtr ptr) {
      // Get dimensions of the texture
      SDL_QueryTexture(ptr, out var _, out var _, out width, out height);
      this.ptr = ptr;
    }

    ~TextureHandle() {
      Console.WriteLine("Destroying loaded texture");
      SDL_DestroyTexture(ptr);
    }
  }

  public class TextureData : IDisposable {
    private readonly IntPtr renderer;
    private readonly Dictionary<string, WeakReference<TextureHandle>> references;

    public TextureData(IntPtr renderer) {
      this.renderer = renderer;
      references = new Dictionary<string, WeakReference<TextureHandle>>();
    }

    public void Dispose() {
      references.Clear();
    }

    public TextureHandle Get(string key) {
      if (references.TryGetValue(key, out var weakRef)) {
        if (weakRef.TryGetTarget(out var handle)) {
          return handle;
        }
        weakRef.SetTarget(Load(key));
        weakRef.TryGetTarget(out var loaded);
        return loaded;
      }
      var newWeakRef = new WeakReference<TextureHandle>(Load(key));
      references.Add(key, newWeakRef);
      newWeakRef.TryGetTarget(out var newHandle);
      return newHandle;
    }

    private TextureHandle Load(string key) {
      var filename = Path.Combine("assets", key + ".png");
      var ptr = IMG_LoadTexture(renderer, filename);
      return new TextureHandle(ptr);
    }
  }
}
