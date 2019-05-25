using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class InGameSettingsPage : UIPage
    {
        public Button closeButton;

        public Button stopGameButton;

        private void Awake() 
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            stopGameButton.onClick.AddListener(OnStopGameButtonClicked);
        }
        
        private void OnCloseButtonClicked()
        {
            m_gameManager.uiManager.HidePage();
        }

        private void OnStopGameButtonClicked()
        {
            m_gameManager.uiManager.HidePage();
            
            m_gameManager.StopGame();

            m_gameManager.uiManager.ShowPage<LevelPage>();
        }
    }
}
