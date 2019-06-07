namespace Gfen.Game.Logic
{
    public class DestroyCommand : Command
    {
        private LogicGameManager m_logicGameManager;

        private Block m_block;

        public DestroyCommand(LogicGameManager logicGameManager, Block block)
        {
            m_logicGameManager = logicGameManager;
            m_block = block;
        }

        protected override void OnPerform()
        {
            m_logicGameManager.RemoveBlock(m_block);
        }

        protected override void OnUndo()
        {
            m_logicGameManager.AddBlock(m_block);
        }
    }
}
