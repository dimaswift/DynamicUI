namespace DynamicUI
{
    using UnityEngine;
    using System.Collections;

    public class DUIAnimated : DUIElement
    {
        [SerializeField]
        protected AnimationOptions m_animationOptions = new AnimationOptions();

        bool m_isAnimating;
        float m_animTime;
        Vector3 m_hiddenPos, m_visiblePos;
        
        public bool isAnimating { get { return m_isAnimating; } }

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

        /// <summary>
        /// No need to call base function.
        /// </summary>
        /// <param name="curveValue">The evaluated value of the curve.</param>
        protected virtual void OnAnimate(float curveValue) { }

        /// <summary>
        /// Called before anination starts. No need to call base function.
        /// </summary>
        protected virtual void OnAnimationEnded() { }

        /// <summary>
        /// Called after anination ends. No need to call base function.
        /// </summary>
        protected virtual void OnAnimationStarted()  {  }

        /// <summary>
        /// Shows element using animation.
        /// </summary>
        public void ShowWithAnimation()
        {
            if (!m_visible)
            {
                Show();
                StartAnimation();
            }
        }
        /// <summary>
        /// Hides element using animation.
        /// </summary>
        public void HideWithAnimation()
        {
            if (m_visible)
            {
                OnHide();
                m_visible = false;
                StartAnimation();
            }
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        protected void StartAnimation()
        {
            if (m_isActive)
            {
                m_animTime = m_animationOptions.curveStart;
                OnAnimationStarted();
                StopCoroutine(Animator());
                StartCoroutine(Animator());
            }
        }

        IEnumerator Animator()
        {
            m_isAnimating = true;
            float end = m_animationOptions.curveEnd;
            while (m_animTime <= end)
            {
                m_animTime += Time.unscaledDeltaTime * m_animationOptions.speed * AnimationOptions.MAX_SPEED;
                OnAnimate(m_animationOptions.curve.Evaluate(m_animTime));
                yield return null;
            }
            OnAnimationEnded();
            m_isAnimating = false;
            if (!m_visible && m_disableOnHide)
                SetActive(false);
        }

        [System.Serializable]
        public class AnimationOptions
        {
            public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            [Range(0f, 1f)]
            public float speed = .5f;

            public AnimationOptions() { }

            public AnimationOptions(AnimationCurve curve, float speed) 
            {
                this.speed = speed;
                this.curve = curve;
            }

            public const float MAX_SPEED = 10f;
            public float curveEnd { get { return curve.keys[curve.length - 1].time; } }
            public float curveStart { get { return curve.keys[0].time; } }

        }


    }
}
