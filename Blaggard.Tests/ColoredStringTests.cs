using System;
using System.Text;
using Xunit;

using Blaggard.Common;
using Blaggard.Graphics;

namespace Blaggard.Tests
{
    public class ColoredStringTests
    {
        [Fact]
        public void ColoredString_StringAddition()
        {
            var str = new ColoredString("Hel", Color.blue) + new ColoredString("lo", Color.green);
            str += " ";
            str += "World!";

            Assert.Equal(str.ToString(), "Hello World!");
        }
    }
}