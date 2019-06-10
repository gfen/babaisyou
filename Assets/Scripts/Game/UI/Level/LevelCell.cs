using Gfen.Game.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class LevelCell : UICell
    {
        public Button selectButton;

        public Text nameText;

        public GameObject passFlagGameObject;

        private int m_chapterIndex;

        private int m_levelIndex;

        private void Awake() 
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
        
        public void SetContent(int chapterIndex, int levelIndex)
        {
            m_chapterIndex = chapterIndex;
            m_levelIndex = levelIndex;

            var levelConfig = m_gameManager.gameConfig.chapterConfigs[chapterIndex].levelConfigs[levelIndex];
            nameText.text = string.Format("{0} {1}", levelIndex + 1, levelConfig.levelName);

            passFlagGameObject.SetActive(m_gameManager.LevelManager.IsLevelPassed(chapterIndex, levelIndex));
        }

        private void OnSelectButtonClicked()
        {
            m_gameManager.StartGame(m_chapterIndex, m_levelIndex);
        }
    }
}
