using System;
using UnityEngine;

namespace Gfen.Game.Logic
{
    [Serializable]
    public class Map : ScriptableObject
    {
        public Vector2Int size;

        public MapBlock[] blocks;
    }

    [Serializable]
    public class MapBlock
    {
        public int entityType;

        public Vector2Int position;
    }
}
