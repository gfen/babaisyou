using System.Collections.Generic;

namespace Gfen.Game.Logic
{
    public class EntityTypeIsEntityTypeRule : Rule
    {
        private LogicGameManager m_logicGameManager;

        private int m_originalEntityType;

        private int m_targetEntityType;

        public EntityTypeIsEntityTypeRule(LogicGameManager logicGameManager, int originalEntityType, int targetEntityType)
        {
            m_logicGameManager = logicGameManager;
            m_originalEntityType = originalEntityType;
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

        private void ConverseTargetBlock(Block block, Stack<Command> tickCommands)
        {
            if (m_originalEntityType == block.entityType)
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
