using System;
using System.Collections.Generic;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "GameConfig", menuName = "babaisyou/GameConfig", order = 0)]
    public class GameConfig : ScriptableObject
    {
        public EntityConfig[] entityConfigs;

        public EntityCategoryConfig[] entityCategoryConfigs;

        public LevelConfig[] levelConfigs;

        public GameObject backgroundPrefab;

        public float inputRepeatDelay;

        private Dictionary<int, EntityConfig> entityConfigDict = new Dictionary<int, EntityConfig>();

        private Dictionary<EntityCategory, EntityCategoryConfig> entityCategoryConfigDict = new Dictionary<EntityCategory, EntityCategoryConfig>();

        public void Init()
        {
            entityConfigDict.Clear();
            foreach (var entityConfig in entityConfigs)
            {
                entityConfigDict[entityConfig.type] = entityConfig;
            }
            entityCategoryConfigDict.Clear();
            foreach (var entityCategoryConfig in entityCategoryConfigs)
            {
                entityCategoryConfigDict[entityCategoryConfig.entityCategory] = entityCategoryConfig;
            }
        }

        public EntityConfig GetEntityConfig(int type)
        {
            return entityConfigDict[type];
        }

        public EntityCategoryConfig GetEntityCategoryConfig(EntityCategory entityCategory)
        {
            return entityCategoryConfigDict[entityCategory];
        }
    }
}
