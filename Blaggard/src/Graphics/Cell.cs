using Blaggard.Common;

namespace Blaggard.Graphics
{
    public struct Cell
    {
        public readonly char ch;
        public readonly Color fore;
        public readonly Color back;

        public Cell(char ch, Color fore, Color back)
        {
            this.ch = ch;
            this.fore = fore;
            this.back = back;
        }

        public static readonly Cell Empty = new Cell(' ', Color.white, Color.black);

        public static bool operator ==(Cell c1, Cell c2) => c1.Equals(c2);
        public static bool operator !=(Cell c1, Cell c2) => !c1.Equals(c2);

        public override bool Equals(object obj)
        {
            if (!(obj is Cell c)) return false;

            return this.ch == c.ch && this.fore == c.fore && this.back == c.back;
        }

        public override int GetHashCode()
        {
            int hashCode = 17;
            unchecked
            {
                hashCode = (hashCode * 23) + ch.GetHashCode();
                hashCode = (hashCode * 23) + fore.GetHashCode();
                hashCode = (hashCode * 23) + back.GetHashCode();
            }
            return hashCode;
        }
    }
}