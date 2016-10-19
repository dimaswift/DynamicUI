using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DynamicUI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class DUIRaycaster : Text, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        UnityEvent m_onPointerDown;
        [SerializeField]
        UnityEvent m_onPointerUp;

        public UnityEvent onPointerDown { get { return m_onPointerDown; } }
        public UnityEvent onPointerUp { get { return m_onPointerUp; } }

        public void Press()
        {
            OnPointerUp(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp.Invoke();
        }

        public void RemoveListeners()
        {
            m_onPointerDown = null;
            m_onPointerUp = null;
        }
    }
}

