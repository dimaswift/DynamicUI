namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;

    public class DUIButton : DUIElement, IPointerDownHandler
    {
        [SerializeField]
        UnityEvent m_onClick;

        public UnityEvent onClick { get { return m_onClick; } }


        public void OnPointerDown(PointerEventData data)
        {
            onClick.Invoke();
        }
    }
}
