namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DUICanvas : MonoBehaviour
    {
        RectTransform m_rectTransform;
        bool m_initialized = false;
        DUIAnimated[] m_animatedElements;

        public RectTransform rectTransform
        {
            get {  if (!m_initialized) Init(); return m_rectTransform; }
        }

        public virtual void Init()
        {
            if (!m_initialized)
            {
                m_rectTransform = GetComponent<RectTransform>();
                m_initialized = true;
                m_animatedElements = GetComponentsInChildren<DUIAnimated>(true);
            }
        }

        void Update()
        {
            int lenght = m_animatedElements.Length;
            var delta = Time.unscaledDeltaTime;
            for (var i = 0; i < lenght; i++)
            {
                m_animatedElements[i].ProcessAnimation(delta);
            }
        }

    }
}