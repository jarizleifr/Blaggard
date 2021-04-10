using System.Runtime.CompilerServices;

namespace Blaggard {
  public class ColoredString {
    public string Text { get; init; }
    public Color Color { get; init; }

    public int Length => Text.Length;

    public char this[int index] => Text[index];

    public ColoredString(string text, Color color) =>
        (Text, Color) = (text, color);

    public override string ToString() => Text;
  }

  public class ColoredTextSpan {
    private readonly ColoredString[] arr;
    public int Length { get; private set; }

    public ColoredTextSpan(string formattedText, Color defaultColor, params ColoredString[] args) {
      var text = FormattableStringFactory.Create(formattedText, args);
      Length = text.ToString().Length;

      var split1 = text.Format.Split('{');
      arr = new ColoredString[split1.Length + text.ArgumentCount];
      int i = 0;
      foreach (var s in split1) {
        var split2 = s.Split('}');
        if (split2.Length == 1) {
          arr[i] = new ColoredString(split2[0], defaultColor);
        } else {
          arr[i++] = (ColoredString)text.GetArgument(int.Parse(split2[0]));
          arr[i] = new ColoredString(split2[1], defaultColor);
        }
        i++;
      }
    }

    public struct ColoredTextSpanEnumerator {
      private readonly ColoredString[] arr;
      private int index;

      internal ColoredTextSpanEnumerator(ColoredString[] arr) =>
          (this.arr, index) = (arr, -1);

      public ColoredString Current => arr[index];

      public bool MoveNext() => ++index < arr.Length;

      public void Reset() => index = -1;
    }

    public ColoredTextSpanEnumerator GetEnumerator() => new(arr);
  }
}
