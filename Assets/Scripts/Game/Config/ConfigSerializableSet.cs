using System;
using System.Collections.Generic;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConfigSerializableSet", menuName = "babaisyou/ConfigSerializableSet", order = 0)]
    public class ConfigSerializableSet : ScriptableObject
    {
        public EntityConfig[] entityConfigs;

        public EntityCategoryConfig[] entityCategoryConfigs;

        public RuleConfig[] ruleConfigs;

        public EntityTypeRuleConfig[] entityTypeRuleConfigs;

        public EntityCategoryRuleConfig[] entityCategoryRuleConfigs;

        public AttributeRuleConfig[] attributeRuleConfigs;

        public KeyWordRuleConfig[] keyWordRuleConfigs;

        public LevelConfig[] levelConfigs;

        public GameObject backgroundPrefab;

        private Dictionary<int, EntityConfig> entityConfigDict = new Dictionary<int, EntityConfig>();

        private Dictionary<EntityCategory, EntityCategoryConfig> entityCategoryConfigDict = new Dictionary<EntityCategory, EntityCategoryConfig>();

        private Dictionary<int, RuleConfig> ruleConfigDict = new Dictionary<int, RuleConfig>();

        private Dictionary<int, EntityTypeRuleConfig> entityTypeRuleConfigDict = new Dictionary<int, EntityTypeRuleConfig>();

        private Dictionary<int, EntityCategoryRuleConfig> entityCategoryRuleConfigDict = new Dictionary<int, EntityCategoryRuleConfig>();

        private Dictionary<int, AttributeRuleConfig> attributeRuleConfigDict = new Dictionary<int, AttributeRuleConfig>();

        private Dictionary<int, KeyWordRuleConfig> keyWordRuleConfigDict = new Dictionary<int, KeyWordRuleConfig>();

        private Dictionary<int, LevelConfig> levelConfigDict = new Dictionary<int, LevelConfig>();

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
            ruleConfigDict.Clear();
            foreach (var ruleConfig in ruleConfigs)
            {
                ruleConfigDict[ruleConfig.type] = ruleConfig;
            }
            entityTypeRuleConfigDict.Clear();
            foreach (var entityTypeRuleConfig in entityTypeRuleConfigs)
            {
                entityTypeRuleConfigDict[entityTypeRuleConfig.type] = entityTypeRuleConfig;
            }
            entityCategoryRuleConfigDict.Clear();
            foreach (var entityCategoryRuleConfig in entityCategoryRuleConfigs)
            {
                entityCategoryRuleConfigDict[entityCategoryRuleConfig.type] = entityCategoryRuleConfig;
            }
            attributeRuleConfigDict.Clear();
            foreach (var attributeRuleConfig in attributeRuleConfigs)
            {
                attributeRuleConfigDict[attributeRuleConfig.type] = attributeRuleConfig;
            }
            keyWordRuleConfigDict.Clear();
            foreach (var keyWordRuleConfig in keyWordRuleConfigs)
            {
                keyWordRuleConfigDict[keyWordRuleConfig.type] = keyWordRuleConfig;
            }
            levelConfigDict.Clear();
            foreach (var levelConfig in levelConfigs)
            {
                levelConfigDict[levelConfig.id] = levelConfig;
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

        public RuleConfig GetRuleConfig(int type)
        {
            return ruleConfigDict[type];
        }

        public EntityTypeRuleConfig GetEntityTypeRuleConfig(int type)
        {
            return entityTypeRuleConfigDict[type];
        }

        public EntityCategoryRuleConfig GetEntityCategoryRuleConfig(int type)
        {
            return entityCategoryRuleConfigDict[type];
        }

        public AttributeRuleConfig GetAttributeRuleConfig(int type)
        {
            return attributeRuleConfigDict[type];
        }

        public KeyWordRuleConfig GetKeyWordRuleConfig(int type)
        {
            return keyWordRuleConfigDict[type];
        }

        public LevelConfig GetLevelConfig(int id)
        {
            return levelConfigDict[id];
        }
    }
}