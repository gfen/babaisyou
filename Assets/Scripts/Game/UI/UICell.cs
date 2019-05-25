using UnityEngine;

namespace Gfen.Game.UI
{
    public abstract class UICell : MonoBehaviour
    {
        protected GameManager m_gameManager;

        public void Show(GameManager gameManager)
        {
            m_gameManager = gameManager;
            
            OnShow();
        }

        public void Hide()
        {
            OnHide();
        }

        protected virtual void OnShow()
        {
            gameObject.SetActive(true);
        }

        protected virtual void OnHide()
        {
            gameObject.SetActive(false);
        }
    }
}
