using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Logic
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConfigSerializableSet", menuName = "BabaIsYou/ConfigSerializableSet", order = 0)]
    public class ConfigSerializableSet : ScriptableObject
    {
        public EntityConfig[] entityConfigs;

        public EntityCategoryConfig[] entityCategoryConfigs;

        public RuleConfig[] ruleConfigs;

        public EntityTypeRuleConfig[] entityTypeRuleConfigs;

        public EntityCategoryRuleConfig[] entityCategoryRuleConfigs;

        public AttributeRuleConfig[] attributeRuleConfigs;

        public KeyWordRuleConfig[] keyWordRuleConfigs;

        public MapConfig[] mapConfigs;

        private Dictionary<int, EntityConfig> entityConfigDict = new Dictionary<int, EntityConfig>();

        private Dictionary<EntityCategory, EntityCategoryConfig> entityCategoryConfigDict = new Dictionary<EntityCategory, EntityCategoryConfig>();

        private Dictionary<int, RuleConfig> ruleConfigDict = new Dictionary<int, RuleConfig>();

        private Dictionary<int, EntityTypeRuleConfig> entityTypeRuleConfigDict = new Dictionary<int, EntityTypeRuleConfig>();

        private Dictionary<int, EntityCategoryRuleConfig> entityCategoryRuleConfigDict = new Dictionary<int, EntityCategoryRuleConfig>();

        private Dictionary<int, AttributeRuleConfig> attributeRuleConfigDict = new Dictionary<int, AttributeRuleConfig>();

        private Dictionary<int, KeyWordRuleConfig> keyWordRuleConfigDict = new Dictionary<int, KeyWordRuleConfig>();

        private Dictionary<int, MapConfig> mapConfigDict = new Dictionary<int, MapConfig>();

        public void Init()
        {
            foreach (var entityConfig in entityConfigs)
            {
                entityConfigDict[entityConfig.type] = entityConfig;
            }
            foreach (var entityCategoryConfig in entityCategoryConfigs)
            {
                entityCategoryConfigDict[entityCategoryConfig.entityCategory] = entityCategoryConfig;
            }
            foreach (var ruleConfig in ruleConfigs)
            {
                ruleConfigDict[ruleConfig.type] = ruleConfig;
            }
            foreach (var entityTypeRuleConfig in entityTypeRuleConfigs)
            {
                entityTypeRuleConfigDict[entityTypeRuleConfig.type] = entityTypeRuleConfig;
            }
            foreach (var entityCategoryRuleConfig in entityCategoryRuleConfigs)
            {
                entityCategoryRuleConfigDict[entityCategoryRuleConfig.type] = entityCategoryRuleConfig;
            }
            foreach (var attributeRuleConfig in attributeRuleConfigs)
            {
                attributeRuleConfigDict[attributeRuleConfig.type] = attributeRuleConfig;
            }
            foreach (var keyWordRuleConfig in keyWordRuleConfigs)
            {
                keyWordRuleConfigDict[keyWordRuleConfig.type] = keyWordRuleConfig;
            }
            foreach (var mapConfig in mapConfigs)
            {
                mapConfigDict[mapConfig.id] = mapConfig;
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

        public MapConfig GetMapConfig(int id)
        {
            return mapConfigDict[id];
        }
    }
}