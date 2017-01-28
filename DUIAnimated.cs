namespace DynamicUI
{
    using UnityEngine;
    using System.Collections;

    public class DUIAnimated : DUIElement
    {
        [SerializeField]
        protected AnimationSettings m_animationSettings = new AnimationSettings();
        [SerializeField]
        [EnumFlag]
        protected AnimationFlags m_animationFlags;

        bool m_isAnimating;
        float m_animTime;
        Vector3 m_hiddenPos, m_visiblePos;

        [System.Flags]
        public enum AnimationFlags { Position = 1, Scale = 2, Alpha = 4 }

        public AnimationFlags animationFlags { get { return m_animationFlags; } set { m_animationFlags = value; } }

        public bool isAnimating { get { return m_isAnimating; } }


        public AnimationSettings animationSettings
        {
            get
            {
                return m_animationSettings;
            }
            set
            {
                m_animationSettings = value;
            }
        }

        public bool hasAnimation
        {
            get
            {
                return HasAnamtionFlag(AnimationFlags.Alpha) ||
                        HasAnamtionFlag(AnimationFlags.Position) ||
                        HasAnamtionFlag(AnimationFlags.Scale);
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
        /// Hides element using animation.
        /// </summary>
        public override void Hide()
        {
            if(hasAnimation == false)
            {
                base.Hide();
                return;
            }
            if (m_visible)
            {
                OnScreenWillHideAnimated();
                m_visible = false;
                StartAnimation();
            }
        }

        public override void Show()
        {
            if (hasAnimation == false)
            {
                base.Show();
                return;
            }
            if (!m_visible)
            {
                OnScreenWillShowAnimated();
                SetActive(true);
                m_visible = true;
                StartAnimation();
            }
        }

        public bool HasAnamtionFlag(AnimationFlags opt)
        {
            return (m_animationFlags & opt) == opt;
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        protected void StartAnimation()
        {
            if (m_isActive)
            {
                m_animTime = m_animationSettings.curveStart;
                OnAnimationStarted();
                m_isAnimating = true;
            }
        }

        /// <summary>
        /// Animation thread. Put it in Update. Usually handled by DUICanvas.
        /// </summary>
        /// <param name="delta">Delta value.</param>
        public void ProcessAnimation(float delta)
        {
            if(m_isAnimating)
            {
                float end = m_animationSettings.curveEnd;
                if (m_animTime <= end)
                {
                    m_animTime += delta * m_animationSettings.speed * AnimationSettings.MAX_SPEED;
                    OnAnimate(m_animationSettings.curve.Evaluate(m_animTime));
                }
                else
                {
                    OnAnimationEnded();
                    m_isAnimating = false;
                    if (!m_visible && m_disableOnHide)
                        SetActive(false);
                }
            }
        }

        [System.Serializable]
        public class AnimationSettings
        {
            public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            [Range(0f, 1f)]
            public float speed = .5f;

            public AnimationSettings() { }

            public AnimationSettings(AnimationCurve curve, float speed) 
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
