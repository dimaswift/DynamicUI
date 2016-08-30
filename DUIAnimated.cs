namespace DynamicUI
{
    using UnityEngine;
    using System.Collections;

    public class DUIAnimated : DUIElement
    {
        [SerializeField]
        protected AnimationOptions m_animationOptions = new AnimationOptions();

        float m_animTime;
        Vector3 m_hiddenPos, m_visiblePos;
        

        public AnimationOptions animationOptions
        {
            get
            {
                return m_animationOptions;
            }
            set
            {
                m_animationOptions = value;
            }
        }

        public void SetHiddenPosition()
        {
            var parentSize = parentCanvas.rectTransform.sizeDelta;
            switch (m_animationOptions.hiddenPosition)
            {
                case Side.Top:
                    m_hiddenPos = new Vector2(0, parentSize.y);
                    break;
                case Side.Bottom:
                    m_hiddenPos = new Vector2(0, -parentSize.y);
                    break;
                case Side.Left:
                    m_hiddenPos = new Vector2(-parentSize.x, 0);
                    break;
                case Side.Right:
                    m_hiddenPos = new Vector2(parentSize.x, 0);
                    break;
                default:
                    break;
            }
        }
        protected virtual void OnAnimate(float curveValue)
        {

        }

        protected virtual void OnAnimationEnded()
        {

        }

        protected virtual void OnAnimationStarted()
        {

        }

        /// <summary>
        /// Shows element using animation
        /// </summary>
        public void ShowWithAnimation()
        {
            if (!m_visible)
            {
                OnShow();
                m_visible = true;
                StopCoroutine(Animator());
                StartCoroutine(Animator());
            }
        }
        /// <summary>
        /// Hides element using animation
        /// </summary>
        public void HideWithAnimation()
        {
            if (m_visible)
            {
                OnHide();
                m_visible = false;
                StopCoroutine(Animator());
                StartCoroutine(Animator());
            }
        }

        protected void StartAnimation()
        {
            if (m_isActive)
            {
                m_animTime = m_animationOptions.curveStart;
                StopCoroutine(Animator());
                StartCoroutine(Animator());
            }
        }

        IEnumerator Animator()
        {
            OnAnimationStarted();
            float end = m_animationOptions.curveEnd;
            while (m_animTime <= end)
            {
                m_animTime += Time.unscaledDeltaTime * m_animationOptions.speed * AnimationOptions.MAX_SPEED;
                OnAnimate(m_animationOptions.curve.Evaluate(m_animTime));
                yield return null;
            }
            OnAnimationEnded();
            if (!m_visible && m_disableOnHide)
                gameObject.SetActive(false);
        }

        [System.Serializable]
        public class AnimationOptions
        {
            public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            public Side hiddenPosition;
            [Range(0f, 1f)]
            public float speed = .5f;
            [EnumFlag]
            public AnimationFlags flags;

            public AnimationOptions() { }

            public AnimationOptions(AnimationCurve curve, float speed, AnimationFlags flags) 
            {
                this.speed = speed;
                this.curve = curve;
                this.flags = flags;
            }

            public const float MAX_SPEED = 10f;
            public float curveEnd { get { return curve.keys[curve.length - 1].time; } }
            public float curveStart { get { return curve.keys[0].time; } }

            [System.Flags]
            public enum AnimationFlags
            {
                XScale = 1,
                YScale = 2,
                Alpha = 4,
                XPos = 8,
                YPos = 16
            }
        }
    }
}
