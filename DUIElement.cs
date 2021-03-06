﻿namespace DynamicUI
{
    using UnityEngine;
    using System.Collections;

    public enum Side
    {
        Top = 0,
        Bottom = 1,
        Left = 2,
        Right = 3
    }

    public class DUIElement : MonoBehaviour
    {
        // Editor exposed members:

        [SerializeField]
        protected bool m_initializeOnAwake = false;
        [SerializeField]
        protected bool m_disableOnHide = true;

        // Protected members:

        protected bool m_visible = false;
        protected bool m_isActive = false;
        protected bool m_initialized = false;
        protected RectTransform m_rectTransform = null;
        protected DUICanvas m_parentCanvas;

        public event System.Action onShow;
        public event System.Action onHide;

        public DUICanvas parentCanvas
        {
            get { return m_parentCanvas; }
        }

        public RectTransform rectTransform
        {
            get
            {
                if (!m_initialized)
                    m_rectTransform = GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }

        public bool isVisible
        {
            get
            {
                return m_visible;
            }
        }

        void Awake()
        {
            if (m_initializeOnAwake && !m_initialized)
                Init(GetComponentInParent<DUICanvas>());
        }

        /// <summary>
        /// Main initialization function. Don't forget to call base when overriding!
        /// </summary>
        public virtual void Init(DUICanvas canvas)
        {
            if(!m_initialized)
            {
                m_parentCanvas = canvas;
                m_rectTransform = GetComponent<RectTransform>();
                m_isActive = gameObject.activeSelf;
                m_visible = m_isActive;
                m_initialized = true;
            } 
        }

        public void Init()
        {
            if (m_initialized) return;
                Init(GetComponentInParent<DUICanvas>());
        }

        /// <summary>
        /// Shows element.
        /// </summary>
        public virtual void Show()
        {
            if (!m_visible)
            {
                if (onShow != null)
                    onShow();
                SetActive(true);
                m_visible = true;
            }
        }
        public void ShowImmediately()
        {
            SetActive(true);
            m_visible = true;
        }
        public void HideImmediately()
        {
            SetActive(false);
            m_visible = false;
        }
        /// <summary>
        /// Hides element.
        /// </summary>
        public virtual void Hide()
        {
            if (m_visible)
            {
                if (onHide != null)
                    onHide();
                m_visible = false;
                if (m_disableOnHide)
                    SetActive(false);
            }
        }

        /// <summary>
        /// Sets gameObject active or not. Ignores call if last call argument and current call argument are the same.
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (active != m_isActive)
            {
                m_isActive = active;
                gameObject.SetActive(m_isActive);
            }
        }

        protected virtual void OnScreenWillShowAnimated() { }

        protected virtual void OnScreenWillHideAnimated() { }

    }

    public class EnumFlagAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagAttribute() { }

        public EnumFlagAttribute(string name)
        {
            enumName = name;
        }
    }
}
