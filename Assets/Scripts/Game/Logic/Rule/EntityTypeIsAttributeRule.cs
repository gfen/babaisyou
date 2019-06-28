namespace Gfen.Game.Logic
{
    public class EntityTypeIsAttributeRule : Rule
    {
        private LogicGameManager m_logicGameManager;

        private int m_entityType;

        private AttributeCategory m_attributeCategory;

        public EntityTypeIsAttributeRule(LogicGameManager logicGameManager, int entityType, AttributeCategory attributeCategory)
        {
            m_logicGameManager = logicGameManager;
            m_entityType = entityType;
            m_attributeCategory = attributeCategory;
        }

        protected override void OnApplyPersistent()
        {
            m_logicGameManager.AttributeHandler.SetAttributeForEntityType(m_entityType, m_attributeCategory);
        }

        protected override bool OnEquals(Rule other)
        {
            var otherEntityTypeIsAttributeRule = other as EntityTypeIsAttributeRule;
            if (otherEntityTypeIsAttributeRule == null)
            {
                return false;
            }

            return m_entityType == otherEntityTypeIsAttributeRule.m_entityType && m_attributeCategory == otherEntityTypeIsAttributeRule.m_attributeCategory;
        }
    }
}
