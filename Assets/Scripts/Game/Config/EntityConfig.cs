using System;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Config
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