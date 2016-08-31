namespace DynamicUI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class DUIAnimatedPanel : DUIAnimated
    {
        [SerializeField]
        protected Side m_hideSide;

        public Side hideSide { get { return m_hideSide; } set { m_hideSide = value; } }

        protected Vector2 m_hiddenPosition, m_visiblePosition;

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_hiddenPosition = GetHiddenPos();
            m_visiblePosition = GetVisiblePos();
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
                    return new Vector2(parentSize.x, 0);
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
            Vector2 pos;
            if(m_visible)
            {
                pos = Vector3.Lerp(m_hiddenPosition, m_visiblePosition, curveValue);
            }
            else
            {
                pos = Vector3.Lerp(m_visiblePosition, m_hiddenPosition, curveValue);
            }
            rectTransform.anchoredPosition = pos;
        }

        protected override void OnAnimationStarted()
        {
            m_hiddenPosition = GetHiddenPos();
            m_visiblePosition = GetVisiblePos();
        }

    }
}