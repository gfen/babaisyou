using System;
using Gfen.Game.Logic;

namespace Gfen.Game.Config
{
    [Serializable]
    public class EntityCategoryConfig
    {
        public EntityCategory entityCategory;

        public AttributeCategory[] inherentAttributeCategories;
    }
}
