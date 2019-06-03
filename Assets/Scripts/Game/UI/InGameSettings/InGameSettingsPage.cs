using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class InGameSettingsPage : UIPage
    {
        public Button closeButton;

        public Button stopGameButton;

        public Button restartGameButton;

        private void Awake() 
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            stopGameButton.onClick.AddListener(OnStopGameButtonClicked);
            restartGameButton.onClick.AddListener(OnRestartGameButtonClicked);
        }
        
        private void OnCloseButtonClicked()
        {
            m_gameManager.ResumeGame();
        }

        private void OnStopGameButtonClicked()
        {
            m_gameManager.StopGame();

            m_gameManager.uiManager.ShowPage<LevelPage>();
        }

        private void OnRestartGameButtonClicked()
        {
            m_gameManager.RestartGame();
        }
    }
}
