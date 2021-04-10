namespace Blaggard {
  public struct Cell {
    public char ch;
    public Color fore;
    public Color back;

    public Cell(char ch, Color fore, Color back) {
      this.ch = ch;
      this.fore = fore;
      this.back = back;
    }

    public static readonly Cell Empty = new(' ', Color.white, Color.black);
  }
}
