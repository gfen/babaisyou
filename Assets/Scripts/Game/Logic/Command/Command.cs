namespace Gfen.Game.Logic
{
    public abstract class Command
    {
        public void Perform()
        {
            OnPerform();
        }

        public void Undo()
        {
            OnUndo();
        }

        protected abstract void OnPerform();

        protected abstract void OnUndo();
    }
}