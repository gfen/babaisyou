namespace Gfen.Game.Logic
{
    public class EntityCategoryIsAttributeRule : Rule
    {
        private LogicGameManager m_logicGameManager;

        private EntityCategory m_entityCategory;

        private AttributeCategory m_attributeCategory;

        public EntityCategoryIsAttributeRule(LogicGameManager logicGameManager, EntityCategory entityCategory, AttributeCategory attributeCategory)
        {
            m_logicGameManager = logicGameManager;
            m_entityCategory = entityCategory;
            m_attributeCategory = attributeCategory;
        }

        protected override void OnApplyPersistent()
        {
            m_logicGameManager.AttributeHandler.SetAttributeForEntityCategory(m_entityCategory, m_attributeCategory);
        }

        protected override bool OnEquals(Rule other)
        {
            var otherEntityCategoryIsAttributeRule = other as EntityCategoryIsAttributeRule;
            if (otherEntityCategoryIsAttributeRule == null)
            {
                return false;
            }

            return m_entityCategory == otherEntityCategoryIsAttributeRule.m_entityCategory && m_attributeCategory == otherEntityCategoryIsAttributeRule.m_attributeCategory;
        }
    }
}
