using System;

namespace Gfen.Game.Logic
{
    [Serializable]
    public class EntityCategoryConfig
    {
        public EntityCategory entityCategory;

        public AttributeCategory[] inherentAttributeCategories;
    }
}
