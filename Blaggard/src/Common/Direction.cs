using System;

namespace Blaggard.Common
{
    public enum Direction
    {
        SouthWest,
        South,
        SouthEast,
        West,
        East,
        NorthWest,
        North,
        NorthEast
    }

    public static class DirectionExtensions
    {
        public static Direction GetOpposite(this Direction dir) => dir switch
        {
            Direction.SouthWest => Direction.NorthEast,
            Direction.South => Direction.North,
            Direction.SouthEast => Direction.NorthWest,
            Direction.West => Direction.East,
            Direction.East => Direction.West,
            Direction.NorthWest => Direction.SouthEast,
            Direction.North => Direction.South,
            Direction.NorthEast => Direction.SouthWest,
            _ => throw new Exception("Invalid value in direction enum")
        };
    }
}