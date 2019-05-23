using System;
using UnityEngine;

namespace Gfen.Game.Logic
{
    [Serializable]
    public class EntityConfig
    {
        public int type;

        public int subyType;

        public EntityCategory category;

        public GameObject prefab;
    }
}