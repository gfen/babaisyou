using System;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    public class MapConfig : ScriptableObject
    {
        public Vector2Int size;

        public MapBlockConfig[] blocks;
    }
}
