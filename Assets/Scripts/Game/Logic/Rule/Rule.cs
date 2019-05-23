using System.Collections.Generic;

namespace Gfen.Game.Logic
{
    public abstract class Rule
    {
        public void ApplyPersistent()
        {
            OnApplyPersistent();
        }

        public void ApplyAction(Stack<Command> tickCommands)
        {
            OnApplyAction(tickCommands);
        }

        protected virtual void OnApplyPersistent()
        {

        }

        protected virtual void OnApplyAction(Stack<Command> tickCommands)
        {

        }
    }
}