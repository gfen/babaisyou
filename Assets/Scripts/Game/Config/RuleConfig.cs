using System;
using Gfen.Game.Logic;

namespace Gfen.Game.Config
{
    [Serializable]
    public class RuleConfig
    {
        public int type;

        public int subType;

        public RuleCategory ruleCategory;
    }
}
