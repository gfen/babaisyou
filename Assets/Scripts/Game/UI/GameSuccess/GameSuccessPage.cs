using UnityEngine.UI;

namespace Gfen.Game.UI
{
    public class GameSuccessPage : UIPage
    {
        public Button okButton;

        private void Awake() 
        {
            okButton.onClick.AddListener(OnOkButtonClicked);
        }
        
        private void OnOkButtonClicked()
        {
            m_gameManager.uiManager.HidePage();
            
            m_gameManager.StopGame();

            m_gameManager.uiManager.ShowPage<LevelPage>();
        }
    }
}
