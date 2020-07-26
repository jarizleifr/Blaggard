using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blaggard.Common;

namespace Blaggard.Graphics
{
    public class ColoredGlyph
    {
        public readonly char ch;
        public readonly Color? color;

        public ColoredGlyph(char ch, Color? color)
        {
            this.ch = ch;
            this.color = color;
        }
    }

    public class ColoredString : IEnumerable<ColoredGlyph>
    {
        private ColoredGlyph[] glyphArray;

        public ColoredString(string text, Color? color)
        {
            glyphArray = text.Select(ch => new ColoredGlyph(ch, color)).ToArray();
        }

        public ColoredString(params ColoredString[] strings)
        {
            glyphArray = strings.SelectMany(s => s).ToArray();
        }

        public static ColoredString Empty => new ColoredString("", null);

        public ColoredGlyph this[int index] => glyphArray[index];
        public int Length => glyphArray.Length;

        public override string ToString() => new string(glyphArray.Select(g => g.ch).ToArray());

        public IEnumerator<ColoredGlyph> GetEnumerator() => glyphArray.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public static ColoredString operator +(ColoredString a, string b)
        {
            var color = a.glyphArray.Last().color;
            a.glyphArray = a.glyphArray.Concat(b.Select(ch => new ColoredGlyph(ch, color))).ToArray();
            return a;
        }

        public static ColoredString operator +(ColoredString a, ColoredString b)
        {
            a.glyphArray = a.glyphArray.Concat(b.glyphArray).ToArray();
            return a;
        }

        public static ColoredString operator +(string a, ColoredString b)
        {
            var color = b.glyphArray.First().color;
            b.glyphArray = a.Select(ch => new ColoredGlyph(ch, color)).Concat(b.glyphArray).ToArray();
            return b;
        }
    }

    public class ColoredStringBuilder
    {
        private static readonly Regex regex = new Regex(@"({\d})");
        private string text;
        private List<ColoredString> parameters;

        public ColoredStringBuilder(string text)
        {
            this.text = text;
            parameters = new List<ColoredString>();
        }

        public ColoredStringBuilder WithParam(ColoredString coloredString)
        {
            parameters.Add(coloredString);
            return this;
        }

        public ColoredStringBuilder WithParam(string text, Color color)
        {
            parameters.Add(new ColoredString(text, color));
            return this;
        }

        public ColoredStringBuilder WithParams(IEnumerable<ColoredString> coloredStrings)
        {
            foreach (var str in coloredStrings)
            {
                WithParam(str);
            }
            return this;
        }

        public ColoredString Build()
        {
            var paramCount = regex.Matches(text).Count;
            if (paramCount != parameters.Count)
            {
                throw new Exception(string.Format("ColoredString expected {0} parameters, {1} found!", paramCount, parameters.Count));
            }

            int index = 0;
            var temp = regex.Split(text).Select(s => regex.IsMatch(s)
                ? parameters[index++]
                : new ColoredString(s, null));

            return new ColoredString(temp.ToArray());
        }
    }
}