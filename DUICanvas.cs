namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DUICanvas : MonoBehaviour
    {
        RectTransform m_rectTransform;
        bool m_initialized = false;

        public RectTransform rectTransform
        {
            get {  if (!m_initialized) Init(); return m_rectTransform; }
        }

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            if(!m_initialized)
            {
                m_rectTransform = GetComponent<RectTransform>();
                m_initialized = true;
            }
        }
    }
}