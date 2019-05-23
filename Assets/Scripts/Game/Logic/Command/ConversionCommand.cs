namespace Gfen.Game.Logic
{
    public class ConversionCommand : Command
    {
        private LogicGameManager m_logicGameManager;

        private Block m_block;

        private int m_targetEntityType;

        private int m_originalEntityType;

        public ConversionCommand(LogicGameManager logicGameManager, Block block, int targetEntityType)
        {
            m_logicGameManager = logicGameManager;
            m_block = block;
            m_targetEntityType = targetEntityType;

            m_originalEntityType = block.entityType;
        }

        protected override void OnPerform()
        {
            m_logicGameManager.ConverseBlockEntityType(m_block, m_targetEntityType);
        }

        protected override void OnUndo()
        {
            m_logicGameManager.ConverseBlockEntityType(m_block, m_originalEntityType);
        }
    }
}
