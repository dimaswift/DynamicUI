using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace DynamicUI
{
    public class DUISwitcher : DUIElement, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        bool m_isOn;
        [SerializeField]
        RectTransform m_handle;
        [SerializeField]
        RectTransform m_handleTravelArea;
        [SerializeField]
        Image m_fill;
        [SerializeField]
        float m_fillStart = 0.22f;
        [SerializeField]
        float m_fillEnd = 0.779f;
        [SerializeField]
        Toggle.ToggleEvent m_onValueChanged;

        Vector2 m_pressedMouse;
        Vector2 m_pressedHandle;
        bool m_isDragging;
        float m_travelAreaWidth;
        bool m_animating;
        float m_dragDelta;
        bool m_isAnimating;
        float m_animationTime;

        public Toggle.ToggleEvent onValueChanged { get { return m_onValueChanged; } }

        public bool isOn
        {
            get { return m_isOn; }
            set { SetIsOn(value); }
        }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_fill.fillMethod = Image.FillMethod.Horizontal;
            m_fill.type = Image.Type.Filled;
            m_travelAreaWidth = m_handleTravelArea.sizeDelta.x;
            SetStateImmediately(isOn);
        }

        void AnimationRoutine()
        {

        }



        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Toggle();
            }
            if(m_isAnimating)
            {
                m_animationTime += Time.deltaTime;
                if (m_animationTime > 2)
                    m_isAnimating = false;
                MoveHandle(Time.deltaTime * 10);
            }
            if(m_isDragging)
            {
                var pointer = Input.mousePosition;
                var delta = m_pressedMouse - (Vector2)pointer;
                delta.y = 0;
                m_handle.position = m_pressedHandle - delta;
                var anchoredPos = m_handle.anchoredPosition;
                anchoredPos.x = Mathf.Clamp(anchoredPos.x, -m_travelAreaWidth, 0);
                m_handle.anchoredPosition = anchoredPos;
                m_fill.fillAmount = Helper.Remap(anchoredPos.x, -m_travelAreaWidth, 0, m_fillStart, m_fillEnd);
                m_dragDelta = ((m_pressedHandle.x - m_handle.position.x) / parentCanvas.rectTransform.localScale.x) / m_travelAreaWidth;
            }
        }

        void MoveHandle(float delta)
        {
            var anchoredPos = m_handle.anchoredPosition;
            if(m_isOn)
                anchoredPos.x = 0;
            else anchoredPos.x = -m_travelAreaWidth;
            m_handle.anchoredPosition = Vector2.Lerp(m_handle.anchoredPosition, anchoredPos, delta) ;
            m_fill.fillAmount = Helper.Remap(m_handle.anchoredPosition.x, -m_travelAreaWidth, 0, m_fillStart, m_fillEnd);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if(!m_isDragging)
            {
                m_pressedHandle = m_handle.position;
                m_pressedMouse = data.position;
                m_isDragging = true;
            }
        }

        public void Toggle()
        {
            isOn = !isOn;
        }

        void SetIsOn(bool isOn)
        {
            m_isOn = isOn;
            m_isAnimating = true;
            m_animationTime = 0;
            m_onValueChanged.Invoke(isOn);
        }

        public void SetStateImmediately(bool isOn)
        {
            m_onValueChanged.Invoke(isOn);
            m_isOn = isOn;
            MoveHandle(float.MaxValue);
        }

        public void OnPointerUp(PointerEventData data)
        {
            m_isDragging = false;

            if (Mathf.Abs(m_dragDelta) < .1f)
                Toggle();
            if (Mathf.Abs(m_dragDelta) > .5f)
            {
                SetIsOn(m_dragDelta < 0);
            }
        }
    }
}
