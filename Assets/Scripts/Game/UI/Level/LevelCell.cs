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

        private LevelConfig m_levelConfig;
        
        public void SetContent(LevelConfig levelConfig)
        {
            m_levelConfig = levelConfig;

            nameText.text = m_levelConfig.name;

            passFlagGameObject.SetActive(m_gameManager.LevelManager.IsLevelPassed(m_levelConfig.id));

            selectLevelButton.onClick.RemoveAllListeners();
            selectLevelButton.onClick.AddListener(OnSelectLevelButtonClicked);
        }

        private void OnSelectLevelButtonClicked()
        {
            m_gameManager.uiManager.HidePage();
            m_gameManager.StartGame(m_levelConfig.id);
        }
    }
}
