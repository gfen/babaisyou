using System;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "EntityConfig", menuName = "babaisyou/EntityConfig", order = 0)]
    public class EntityConfig : ScriptableObject
    {
        public int type;
        
        public GameObject prefab;

        public EntityCategory category;

        public RuleCategory ruleCategory;

        public int entityTypeForRule;

        public EntityCategory entityCategoryForRule;

        public AttributeCategory attributeCategoryForRule;

        public KeyWordCategory keyWordCategoryForRule;
    }
}
