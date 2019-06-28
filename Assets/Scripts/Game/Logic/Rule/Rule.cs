using System;
using System.Collections.Generic;

namespace Gfen.Game.Logic
{
    public abstract class Rule : IEquatable<Rule>
    {
        public void ApplyPersistent()
        {
            OnApplyPersistent();
        }

        public void ApplyAction(Stack<Command> tickCommands)
        {
            OnApplyAction(tickCommands);
        }

        public bool Equals(Rule other)
        {
            return OnEquals(other);
        }

        protected virtual void OnApplyPersistent()
        {

        }

        protected virtual void OnApplyAction(Stack<Command> tickCommands)
        {

        }

        protected abstract bool OnEquals(Rule other);
    }
}