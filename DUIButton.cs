namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class DUIButton : DUIElement
    {
        [SerializeField]
        UnityEvent m_onClick;

        public UnityEvent onClick { get { return m_onClick; } }

    }
}
