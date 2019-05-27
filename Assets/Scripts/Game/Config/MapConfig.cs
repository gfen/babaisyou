using System;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    public class MapConfig : ScriptableObject
    {
        public Vector2Int size;

        public MapBlock[] blocks;
    }

    [Serializable]
    public class MapBlock
    {
        public int entityType;

        public Vector2Int position;

        public Direction direction;
    }
}
