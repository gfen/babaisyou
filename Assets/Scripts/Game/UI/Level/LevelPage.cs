using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.UI
{
    public class LevelPage : UIPage
    {
        public Transform levelListRootTransform;

        public LevelCell templateLevelCell;

        private List<LevelCell> m_levelCells = new List<LevelCell>();

        protected override void OnShow()
        {
            base.OnShow();

            var levelConfigs = m_gameManager.gameConfig.levelConfigs;
            while (m_levelCells.Count < levelConfigs.Length)
            {
                var levelCell = UIUtils.InstantiateUICell(levelListRootTransform, templateLevelCell);
                m_levelCells.Add(levelCell);
            }
            for (var i = 0; i < levelConfigs.Length; i++)
            {
                m_levelCells[i].Show(m_gameManager);
                m_levelCells[i].SetContent(i);
            }
            for (var i = levelConfigs.Length; i < m_levelCells.Count; i++)
            {
                m_levelCells[i].Hide();
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}
