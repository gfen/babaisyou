using System.Collections.Generic;

namespace Gfen.Game.Logic
{
    public class EntityCategoryIsEntityTypeRule : Rule
    {
        private LogicGameManager m_logicGameManager;

        private EntityCategory m_originalEntityCategory;

        private int m_targetEntityType;

        public EntityCategoryIsEntityTypeRule(LogicGameManager logicGameManager, EntityCategory originalEntityType, int targetEntityType)
        {
            m_logicGameManager = logicGameManager;
            m_originalEntityCategory = originalEntityType;
            m_targetEntityType = targetEntityType;
        }

        protected override void OnApplyAction(Stack<Command> tickCommands)
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
                        ConverseTargetBlock(block, tickCommands);
                    }
                }
            }
        }

        protected override bool OnEquals(Rule other)
        {
            var otherEntityCategoryIsEntityTypeRule = other as EntityCategoryIsEntityTypeRule;
            if (otherEntityCategoryIsEntityTypeRule == null)
            {
                return false;
            }

            return m_originalEntityCategory == otherEntityCategoryIsEntityTypeRule.m_originalEntityCategory && m_targetEntityType == otherEntityCategoryIsEntityTypeRule.m_targetEntityType;
        }

        private void ConverseTargetBlock(Block block, Stack<Command> tickCommands)
        {
            var blockEntityCategory = m_logicGameManager.GameManager.gameConfig.GetEntityConfig(block.entityType).category;
            if (m_originalEntityCategory == blockEntityCategory)
            {
                var conversionCommand = new ConversionCommand(m_logicGameManager, block, m_targetEntityType);
                conversionCommand.Perform();
                if (tickCommands != null)
                {
                    tickCommands.Push(conversionCommand);
                }
            }
        }
    }
}
