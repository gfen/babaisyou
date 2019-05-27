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
    }
}
