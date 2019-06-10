using UnityEngine;
using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class ChapterCell : UICell
    {
        public Button selectButton;

        public Text nameText;

        public GameObject passFlagGameObject;

        private int m_chapterIndex;

        private void Awake() 
        {
            selectButton.onClick.AddListener(OnSelectButtonClicked);
        }
        
        public void SetContent(int chapterIndex)
        {
            m_chapterIndex = chapterIndex;

            var chapterConfig = m_gameManager.gameConfig.chapterConfigs[chapterIndex];
            nameText.text = string.Format("{0} {1}", chapterIndex + 1, chapterConfig.chapterName);

            passFlagGameObject.SetActive(m_gameManager.LevelManager.IsChapterPassed(chapterIndex));
        }

        private void OnSelectButtonClicked()
        {
            var levelPage = m_gameManager.uiManager.ShowPage<LevelPage>();
            levelPage.SetContent(m_chapterIndex);
        }
    }
}
