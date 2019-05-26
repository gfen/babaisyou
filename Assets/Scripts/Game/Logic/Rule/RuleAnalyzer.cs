using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public class RuleAnalyzer
    {
        public LogicGameManager m_logicGameManager;

        private readonly RuleCategory[] m_backwardRuleCategories = new RuleCategory[] { RuleCategory.EntityType, RuleCategory.EntityCategory };

        private readonly RuleCategory[] m_forwardRuleCategories = new RuleCategory[] { RuleCategory.EntityType, RuleCategory.Attribute };

        private List<Rule> m_currentRules = new List<Rule>();

        private List<Rule> m_cachedRules = new List<Rule>();

        private List<Block> m_cachedBackwardRuleBlocks = new List<Block>();

        private List<Block> m_cachedForwardRuleBlocks = new List<Block>();

        private List<Block> m_cachedIsKeyWordRuleBlocks = new List<Block>();

        public RuleAnalyzer(LogicGameManager logicGameManager)
        {
            m_logicGameManager = logicGameManager;
        }

        public void Refresh()
        {
            RefreshCurrentRules();

            foreach (var rule in m_currentRules)
            {
                rule.ApplyPersistent();
            }
        }

        public void Apply(Stack<Command> tickCommands)
        {
            RefreshCurrentRules();

            foreach (var rule in m_currentRules)
            {
                rule.ApplyPersistent();
                rule.ApplyAction(tickCommands);
            }
        }

        public void Clear()
        {
            m_currentRules.Clear();
        }

        private void FindIsKeyWordRuleBlocks(List<Block> isBlocks)
        {
            var map = m_logicGameManager.Map;

            var mapXLength = map.GetLength(0);
            var mapYLength = map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    foreach (var block in map[i, j])
                    {
                        if (IsTargetKeyWordRuleBlock(block, KeyWordCategory.Is))
                        {
                            isBlocks.Add(block);
                        }
                    }
                }
            }
        }

        private bool IsTargetKeyWordRuleBlock(Block block, KeyWordCategory keyWordCategory)
        {
            var gameConfig = m_logicGameManager.GameManager.gameConfig;

            var entityConfig = gameConfig.GetEntityConfig(block.entityType);
            if (entityConfig.category != EntityCategory.Rule)
            {
                return false;
            }

            if (entityConfig.ruleCategory != RuleCategory.KeyWord)
            {
                return false;
            }

            if (entityConfig.keyWordCategoryForRule != keyWordCategory)
            {
                return false;
            }

            return true;
        }

        private bool IsTargetRuleBlock(Block block, RuleCategory ruleCategory)
        {
            var gameConfig = m_logicGameManager.GameManager.gameConfig;

            var entityConfig = gameConfig.GetEntityConfig(block.entityType);
            if (entityConfig.category != EntityCategory.Rule)
            {
                return false;
            }

            if (entityConfig.ruleCategory != ruleCategory)
            {
                return false;
            }

            return true;
        }

        private void RefreshCurrentRules()
        {
            FindIsKeyWordRuleBlocks(m_cachedIsKeyWordRuleBlocks);

            FindDirectionRules(Vector2Int.right, m_cachedIsKeyWordRuleBlocks, m_cachedRules);
            FindDirectionRules(Vector2Int.down, m_cachedIsKeyWordRuleBlocks, m_cachedRules);

            m_currentRules.Clear();
            m_currentRules.AddRange(m_cachedRules);

            m_cachedIsKeyWordRuleBlocks.Clear();
            m_cachedRules.Clear();
        }

        private void FindDirectionRules(Vector2Int direction, List<Block> isKeyWordRuleBlocks, List<Rule> resultRules)
        {
            var configSet = m_logicGameManager.GameManager.gameConfig;

            foreach (var isKeyWordRuleBlock in isKeyWordRuleBlocks)
            {
                FindConnectedRuleBlocks(isKeyWordRuleBlock.position, Vector2Int.zero - direction, m_backwardRuleCategories, m_cachedBackwardRuleBlocks);
                FindConnectedRuleBlocks(isKeyWordRuleBlock.position, direction, m_forwardRuleCategories, m_cachedForwardRuleBlocks);

                foreach (var backwardRuleBlock in m_cachedBackwardRuleBlocks)
                {
                    var backwardEntityConfig = configSet.GetEntityConfig(backwardRuleBlock.entityType);

                    foreach (var forwardRuleBlock in m_cachedForwardRuleBlocks)
                    {
                        var forwardEntityConfig = configSet.GetEntityConfig(forwardRuleBlock.entityType);

                        if (backwardEntityConfig.ruleCategory == RuleCategory.EntityType)
                        {
                            if (forwardEntityConfig.ruleCategory == RuleCategory.EntityType)
                            {
                                resultRules.Add(new EntityTypeIsEntityTypeRule(m_logicGameManager, backwardEntityConfig.entityTypeForRule, forwardEntityConfig.entityTypeForRule));
                            }
                            else if (forwardEntityConfig.ruleCategory == RuleCategory.Attribute)
                            {
                                resultRules.Add(new EntityTypeIsAttributeRule(m_logicGameManager, backwardEntityConfig.entityTypeForRule, forwardEntityConfig.attributeCategoryForRule));
                            }
                        }
                        else if (backwardEntityConfig.ruleCategory == RuleCategory.EntityCategory)
                        {
                            if (forwardEntityConfig.ruleCategory == RuleCategory.EntityType)
                            {
                                resultRules.Add(new EntityCategoryIsEntityTypeRule(m_logicGameManager, backwardEntityConfig.entityCategoryForRule, forwardEntityConfig.entityTypeForRule));
                            }
                            else if (forwardEntityConfig.ruleCategory == RuleCategory.Attribute)
                            {
                                resultRules.Add(new EntityCategoryIsAttributeRule(m_logicGameManager, backwardEntityConfig.entityCategoryForRule, forwardEntityConfig.attributeCategoryForRule));
                            }
                        }
                    }
                }

                m_cachedBackwardRuleBlocks.Clear();
                m_cachedForwardRuleBlocks.Clear();
            }
        }

        private void FindConnectedRuleBlocks(Vector2Int originPosition, Vector2Int direction, RuleCategory[] targetRuleCategories, List<Block> resultBlocks)
        {
            var position = originPosition + direction;
            while (m_logicGameManager.InMap(position))
            {
                var mapBlocks = m_logicGameManager.Map[position.x, position.y];

                var hasTargetRuleBlock = false;
                foreach (var block in mapBlocks)
                {
                    var hasTargetRuleCategory = false;
                    foreach (var targetRuleCategory in targetRuleCategories)
                    {
                        if (IsTargetRuleBlock(block, targetRuleCategory))
                        {
                            hasTargetRuleCategory = true;
                            break;
                        }
                    }
                    if (hasTargetRuleCategory)
                    {
                        hasTargetRuleBlock = true;
                        resultBlocks.Add(block);
                    }
                }

                if (!hasTargetRuleBlock)
                {
                    break;
                }

                position += direction;

                if (!m_logicGameManager.InMap(position))
                {
                    break;
                }

                mapBlocks = m_logicGameManager.Map[position.x, position.y];

                var hasAndKeyWordRuleBlock = false;
                foreach (var block in mapBlocks)
                {
                    if (IsTargetKeyWordRuleBlock(block, KeyWordCategory.And))
                    {
                        hasAndKeyWordRuleBlock = true;
                    }
                }

                if (!hasAndKeyWordRuleBlock)
                {
                    break;
                }

                position += direction;
            }
        }
    }
}
