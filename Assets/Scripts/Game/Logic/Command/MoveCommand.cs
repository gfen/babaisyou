using UnityEngine;

namespace Gfen.Game.Logic
{
    public class MoveCommand : Command
    {
        private LogicGameManager m_logicGameManager;

        private Block m_block;

        private Direction m_targetDirection;

        private int m_targetLength;

        private Vector2Int m_originalPosition;

        private Direction m_originalDirection;

        public MoveCommand(LogicGameManager logicManager, Block block, Direction direction, int length)
        {
            m_logicGameManager = logicManager;
            m_block = block;
            m_targetDirection = direction;
            m_targetLength = length;

            m_originalPosition = block.position;
            m_originalDirection = block.direction;
        }
        
        protected override void OnPerform()
        {
            var displacement = DirectionUtils.DirectionToDisplacement(m_targetDirection)*m_targetLength;

            m_logicGameManager.MoveBlock(m_block, m_block.position + displacement);
            m_block.direction = m_targetDirection;

            if (m_logicGameManager.BlockMoved != null)
            {
                m_logicGameManager.BlockMoved(m_block);
            }
        }

        protected override void OnUndo()
        {
            m_logicGameManager.MoveBlock(m_block, m_originalPosition);
            m_block.direction = m_originalDirection;
        }
    }
}
