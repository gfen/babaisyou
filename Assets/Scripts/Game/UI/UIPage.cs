using UnityEngine;

namespace Gfen.Game.UI
{
    public abstract class UIPage : MonoBehaviour
    {
        public Canvas canvas;
        
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
