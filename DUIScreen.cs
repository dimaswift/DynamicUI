namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DUIScreen : DUIAnimated
    {
        [SerializeField]
        protected Side m_hideSide;
        [SerializeField]
        protected bool m_resetPositionOnLoad = true;
        [SerializeField]
        bool m_showBackButton = true;
        [SerializeField]
        protected bool m_stretchToScreenSize = true;
        [SerializeField]
        protected bool m_isScreen = true;

        protected CanvasGroup m_canvasGroup;

        protected Vector2 m_hiddenPosition, m_visiblePosition, m_visibleScale, m_hiddenScale = Vector3.zero;

        bool m_hasCanvasGroup = false;

        public Side hideSide { get { return m_hideSide; } set { m_hideSide = value; } }

        public DUIScreen prevScreen { get; private set; }

        public bool showBackButton { get { return m_showBackButton; } }

        public bool showedUsingBackButton { get; set; }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_hiddenPosition = GetHiddenPos();
            m_visiblePosition = m_resetPositionOnLoad ? Vector2.zero : rectTransform.anchoredPosition;
            if (m_resetPositionOnLoad)
                Invoke("ResetPos", .5f);
            m_visibleScale = rectTransform.localScale;

            if (HasAnamtionFlag(AnimationFlags.Alpha))
            {
                m_canvasGroup = GetComponent<CanvasGroup>();
                if (m_canvasGroup == null)
                    m_canvasGroup = gameObject.AddComponent<CanvasGroup>();
                m_hasCanvasGroup = true;
            }
        }

        public virtual bool allowUpdatingElements { get { return true; } }

        public void SetPrevScreen(DUIScreen screen)
        {
            prevScreen = screen;
        }

        void ResetPos ()
        {
            if (m_stretchToScreenSize)
                rectTransform.sizeDelta = parentCanvas.rectTransform.sizeDelta;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        public override void Hide()
        {
            base.Hide();
            showedUsingBackButton = false;
        }

        public void ShowPreviousScreen()
        {
            if(prevScreen)
            {
                prevScreen.showedUsingBackButton = true;
                prevScreen.Show();
            }
        }

        public override void Show()
        {
            base.Show();
            if(m_isScreen)
                parentCanvas.SetCurrentScreen(this);
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
            if (HasAnamtionFlag(AnimationFlags.Position))
            {
                MoveToPosition(curveValue);
            }
            if(HasAnamtionFlag(AnimationFlags.Alpha))
            {
                SetAlpha(curveValue);
            }
            if(HasAnamtionFlag(AnimationFlags.Scale))
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
            if(HasAnamtionFlag(AnimationFlags.Position))
            {
                //m_hiddenPosition = GetHiddenPos();
                //m_visiblePosition = GetVisiblePos();
            }
            if(HasAnamtionFlag(AnimationFlags.Alpha))
            {
                m_canvasGroup.alpha = m_visible ? 0 : 1;
            }
        }

    }
}