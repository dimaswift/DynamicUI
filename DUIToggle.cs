using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DynamicUI
{
    public class DUIToggle : DUIAnimated, IPointerDownHandler
    {
        [SerializeField]
        bool m_useAnimation = false;
        [SerializeField]
        bool m_toggleOnClick = false;
        [SerializeField]
        Toggle.ToggleEvent m_onToggleChange;
        [SerializeField]
        UnityEvent m_onClick;
        [SerializeField]
        Color m_onColor = Color.white;
        [SerializeField]
        Color m_offColor = Color.white;
        bool m_isOn = false;
        [SerializeField]
        Sprite m_onSprite = null, m_offSprite = null;
        Image m_toggleGraphic;
        bool m_scalingDown, m_pendingValue;

        public bool isOn
        {
            get { return m_isOn; }
            set
            {
                if(m_isOn != value)
                {
                    if(!m_useAnimation)
                    {
                        m_isOn = value;
                        SetSprite();
                        m_onToggleChange.Invoke(m_isOn);
                    }
                    else
                    {
                        StartAnimation();
                        m_scalingDown = true;
                        m_pendingValue = value;
                    }
                }
            }
        }

        public Toggle.ToggleEvent onToggleChange { get { return m_onToggleChange; } }

        public UnityEvent onClick { get { return m_onClick; } }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_toggleGraphic = GetComponent<Image>();
            SetSprite();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onClick.Invoke();
            if(m_toggleOnClick)
                Toggle();
        }

        public void Toggle()
        {
            isOn = !isOn;
        }

        public void SetStateImmediatedly(bool isOn)
        {
            m_isOn = isOn;
            SetSprite();
        }

        void SetSprite()
        {
            m_toggleGraphic.sprite = m_isOn ? m_onSprite : m_offSprite;
            m_toggleGraphic.color = m_isOn ? m_onColor : m_offColor;
        }

        protected override void OnAnimate(float curveValue)
        {
            if(curveValue < .5f)
            {
                var s = new Vector2(.5f - curveValue, .5f - curveValue) * 2;
                rectTransform.localScale = s;
            }
            else
            {
                if(m_scalingDown)
                {
                    m_scalingDown = false;
                    m_isOn = m_pendingValue;
                    SetSprite();
                    m_onToggleChange.Invoke(m_isOn);
                }
                var s = new Vector2(curveValue - .5f, curveValue - .5f) * 2;
                rectTransform.localScale = s;
            }
        }
    }
}
