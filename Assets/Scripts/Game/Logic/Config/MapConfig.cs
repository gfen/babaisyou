using System;
using UnityEngine;

namespace Gfen.Game.Logic
{
    [Serializable]
    public class MapConfig
    {
        public int id;

        public Vector2Int size;

        public MapBlockConfig[] blocks;
    }

    [Serializable]
    public class MapBlockConfig
    {
        public int entityType;

        public Vector2Int position;
    }
}