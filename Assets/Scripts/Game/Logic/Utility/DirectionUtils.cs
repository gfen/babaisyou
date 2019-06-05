using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public static class DirectionUtils
    {
        public static Vector2Int DirectionToDisplacement(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up: return Vector2Int.up;
                case Direction.Down: return Vector2Int.down;
                case Direction.Left: return Vector2Int.left;
                case Direction.Right: return Vector2Int.right;
                default: return Vector2Int.zero;
            }
        }

        public static Direction DisplacementToDirection(Vector2 displacement)
        {
            if (displacement.x > 0 && displacement.x > Mathf.Abs(displacement.y))
            {
                return Direction.Right;
            }
            else if (displacement.x < 0 && displacement.x < -Mathf.Abs(displacement.y))
            {
                return Direction.Left;
            }
            else if (displacement.y > 0 && displacement.y > Mathf.Abs(displacement.x))
            {
                return Direction.Up;
            }
            else if (displacement.y < 0 && displacement.y < -Mathf.Abs(displacement.x))
            {
                return Direction.Down;
            }

            return Direction.Up;
        }

        private static readonly Dictionary<Direction, HashSet<Direction>> s_parallelDirectionDict = new Dictionary<Direction, HashSet<Direction>>
        {
            { Direction.Up, new HashSet<Direction> { Direction.Up, Direction.Down } },
            { Direction.Down, new HashSet<Direction> { Direction.Up, Direction.Down } },
            { Direction.Left, new HashSet<Direction> { Direction.Left, Direction.Right } },
            { Direction.Right, new HashSet<Direction> { Direction.Left, Direction.Right } },
        };

        public static bool IsParallel(Direction direction1, Direction direction2)
        {
            return s_parallelDirectionDict[direction1].Contains(direction2);
        }

        private static readonly Dictionary<Direction, Direction> s_oppositeDirectionDict = new Dictionary<Direction, Direction>
        {
            { Direction.Up, Direction.Down },
            { Direction.Down, Direction.Up },
            { Direction.Left, Direction.Right },
            { Direction.Right, Direction.Left },
        };

        public static Direction GetOppositeDirection(Direction direction)
        {
            return s_oppositeDirectionDict[direction];
        }
    }
}
