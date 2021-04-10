using System;
using Xunit;

namespace Blaggard.Tests {
  public class RectTests {
    [Fact]
    public void BoundsChecking() {
      var rect = new Rect(5, 5, 10, 10);

      Assert.True(rect.IsWithinBounds(5, 5));
      Assert.False(rect.IsWithinBounds(15, 15));
    }
  }
}
