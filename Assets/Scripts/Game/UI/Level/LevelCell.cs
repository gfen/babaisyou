using Gfen.Game.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class LevelCell : UICell
    {
        public Button selectLevelButton;

        public Text nameText;

        public GameObject passFlagGameObject;

        private int m_levelIndex;

        private void Awake() 
        {
            selectLevelButton.onClick.AddListener(OnSelectLevelButtonClicked);
        }
        
        public void SetContent(int levelIndex)
        {
            m_levelIndex = levelIndex;

            var levelConfig = m_gameManager.gameConfig.levelConfigs[levelIndex];
            nameText.text = levelConfig.name;

            passFlagGameObject.SetActive(m_gameManager.LevelManager.IsLevelPassed(levelIndex));
        }

        private void OnSelectLevelButtonClicked()
        {
            m_gameManager.uiManager.HidePage();
            m_gameManager.StartGame(m_levelIndex);
        }
    }
}
