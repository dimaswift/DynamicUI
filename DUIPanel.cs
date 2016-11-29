namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DUIPanel : DUIAnimated
    {
        [SerializeField]
        protected Side m_hideSide;
        [SerializeField]
        protected bool m_resetPositionOnLoad = true;
        [SerializeField]
        protected DUIButton m_backButton;

        public Side hideSide { get { return m_hideSide; } set { m_hideSide = value; } }
        [EnumFlag]
        [SerializeField]
        protected HideOptions m_hideOptions;
        [SerializeField]
        protected bool m_stretchToScreenSize = true;
        protected CanvasGroup m_canvasGroup;

        public HideOptions hideOptions { get { return m_hideOptions; } set { m_hideOptions = value; } }

        [System.Flags]
        public enum HideOptions { Position = 1, Scale = 2, Alpha = 4 }

        protected Vector2 m_hiddenPosition, m_visiblePosition, m_visibleScale, m_hiddenScale = Vector3.zero;

        bool m_hasCanvasGroup = false;

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_hiddenPosition = GetHiddenPos();
            m_visiblePosition = m_resetPositionOnLoad ? Vector2.zero : rectTransform.anchoredPosition;
            if (m_resetPositionOnLoad)
                Invoke("ResetPos", .5f);
            m_visibleScale = rectTransform.localScale;

            if (HasOption(HideOptions.Alpha))
            {
                m_canvasGroup = GetComponent<CanvasGroup>();
                if (m_canvasGroup == null)
                    m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
                m_hasCanvasGroup = true;
            }
        }

        protected virtual void OnBackPressed()
        {
            
        }

        void ResetPos ()
        {
            if (m_stretchToScreenSize)
                rectTransform.sizeDelta = parentCanvas.rectTransform.sizeDelta;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        protected bool HasOption(HideOptions opt)
        {
            return (m_hideOptions & opt) == opt;
        }

        protected Vector2 GetHiddenPos()
        {
            if (isAnimating && m_visible)
                return rectTransform.anchoredPosition;
            var parentSize = parentCanvas.rectTransform.sizeDelta;
            switch (m_hideSide)
            {
                case Side.Top:
                    return new Vector2(0, parentSize.y);
                case Side.Bottom:
                    return new Vector2(0, -parentSize.y);
                case Side.Left:
                    return new Vector2(-parentSize.x, 0);
                case Side.Right:
                    return new Vector2((parentSize.x * .5f) + (rectTransform.sizeDelta.x * .5f), 0);
            }
            return Vector2.zero;
        }

        protected Vector2 GetVisiblePos()
        {
            if (isAnimating && !m_visible)
                return rectTransform.anchoredPosition;
            return Vector2.zero;
        }

        protected override void OnAnimate(float curveValue)
        {
            if (HasOption(HideOptions.Position))
            {
                MoveToPosition(curveValue);
            }
            if(HasOption(HideOptions.Alpha))
            {
                SetAlpha(curveValue);
            }
            if(HasOption(HideOptions.Scale))
            {
                ScaleToSize(curveValue);
            }
        }

        void ScaleToSize(float curveValue)
        {
            var targetScale = m_visible ? m_visibleScale : m_hiddenScale;
            var originalScale = m_visible ? m_hiddenScale : m_visibleScale;
            rectTransform.localScale = Vector3.Lerp(originalScale, targetScale, curveValue);
        }

        void SetAlpha(float curveValue)
        {
            if(!m_hasCanvasGroup)
            {
                m_hasCanvasGroup = true;
                m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            m_canvasGroup.alpha = m_visible ? curveValue : 1f - curveValue;
        }

        void MoveToPosition(float curveValue)
        {
            var targetPos = m_visible ? m_visiblePosition : m_hiddenPosition;
            var originalPos = m_visible ? m_hiddenPosition : m_visiblePosition;
            rectTransform.anchoredPosition = Vector3.Lerp(originalPos, targetPos, curveValue);
        }

        protected override void OnAnimationStarted()
        {
            if(HasOption(HideOptions.Position))
            {
                //m_hiddenPosition = GetHiddenPos();
                //m_visiblePosition = GetVisiblePos();
            }
            if(HasOption(HideOptions.Alpha))
            {
                m_canvasGroup.alpha = m_visible ? 0 : 1;
            }
        }

    }
}