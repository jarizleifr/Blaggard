using System;
using Xunit;

using Blaggard.Common;

namespace Blaggard.Tests
{
    public class NeighborMatrixTests
    {
        private static int[] testData = new int[]
        {
            1, 1, 1, 1, 1,
            0, 1, 0, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
            1, 1, 1, 1, 1,
        };

        [Fact]
        public void ReturnsExpected()
        {
            var matrix = new NeighborMatrix(new Vec2(1, 1), (origin) => testData[Util.IndexFromVec2(origin, 5)] == 1);

            Assert.True(matrix.IsSet(1 + 2 + 4));
            Assert.False(matrix.IsSet(8 + 128));
            Assert.True(matrix.IsSet(16 + 32 + 64));
        }

        [Fact]
        public void SpecificScenario()
        {
            var matrix = new NeighborMatrix(new Vec2(1, 1), (origin) => testData[Util.IndexFromVec2(origin, 5)] == 1);

            Assert.False(matrix.IsSet(1 + 2 + 4 + 8 + 128));
        }
    }
}
