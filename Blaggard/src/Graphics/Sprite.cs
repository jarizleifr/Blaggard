namespace Blaggard {
  public class Sprite {
    public readonly int x, y, zIndex;
    public TextureHandle Texture { get; private set; }

    public Sprite(int x, int y, TextureHandle texture, int zIndex = 0) {
      this.x = x;
      this.y = y;
      this.zIndex = zIndex;
      Texture = texture;
    }
  }
}
