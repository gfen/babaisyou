using System;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    public class LevelConfig
    {
        public int id;

        public string name;

        public MapConfig map;
    }
}
