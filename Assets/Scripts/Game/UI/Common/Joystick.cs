using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

namespace Gfen.Game.UI
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public RectTransform clickRect;

        public RectTransform moveRect;

        public RectTransform handRect;

        public float movementRange = 100;
		
		public string horizontalAxisName = "Horizontal";
		public string verticalAxisName = "Vertical";

        private bool m_isMoving;
		private Vector2 m_startPos;

		private CrossPlatformInputManager.VirtualAxis m_horizontalVirtualAxis;
		private CrossPlatformInputManager.VirtualAxis m_verticalVirtualAxis;

        private void OnEnable()
        {
            m_isMoving = false;
            moveRect.gameObject.SetActive(m_isMoving);
            handRect.gameObject.SetActive(m_isMoving);

            m_horizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_horizontalVirtualAxis);

            m_verticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_verticalVirtualAxis);
        }

        private void OnDisable()
        {
            m_horizontalVirtualAxis.Remove();
            m_verticalVirtualAxis.Remove();
        }

        public void OnPointerDown(PointerEventData data)
        {
            m_isMoving = RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, data.position, null, out m_startPos);

            moveRect.gameObject.SetActive(m_isMoving);
            handRect.gameObject.SetActive(m_isMoving);

            if (m_isMoving)
            {
                moveRect.anchoredPosition = m_startPos;
                handRect.anchoredPosition = m_startPos;
            }
        }

        public void OnPointerUp(PointerEventData data)
		{
            if (!m_isMoving)
            {
                return;
            }

			moveRect.gameObject.SetActive(false);
            handRect.gameObject.SetActive(false);

			UpdateVirtualAxes(Vector2.zero);
		}

        public void OnDrag(PointerEventData data)
		{
            if (!m_isMoving)
            {
                return;
            }

			var newPos = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, data.position, null, out newPos);

            var delta = (newPos - m_startPos);
            delta.x = Mathf.Clamp(delta.x, -movementRange, movementRange);
            delta.y = Mathf.Clamp(delta.y, -movementRange, movementRange);

            newPos = m_startPos + delta;
			
			handRect.anchoredPosition = newPos;

            delta /= movementRange;
			UpdateVirtualAxes(delta);
		}

        void UpdateVirtualAxes(Vector2 delta)
		{
            m_horizontalVirtualAxis.Update(delta.x);
            m_verticalVirtualAxis.Update(delta.y);
		}
    }
}
