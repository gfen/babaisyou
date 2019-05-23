using UnityEngine;

namespace Gfen.Game.Logic
{
    public class MoveCommand : Command
    {
        private LogicGameManager m_logicGameManager;

        private Block m_block;

        private Vector2Int m_displacement;

        public MoveCommand(LogicGameManager logicManager, Block block, Vector2Int displacement)
        {
            m_logicGameManager = logicManager;
            m_block = block;
            m_displacement = displacement;
        }
        
        protected override void OnPerform()
        {
            m_logicGameManager.SetBlockPosition(m_block, m_block.position + m_displacement);
        }

        protected override void OnUndo()
        {
            m_logicGameManager.SetBlockPosition(m_block, m_block.position - m_displacement);
        }
    }
}
