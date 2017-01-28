using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    public interface ILoadableImage
    {
        Sprite sprite { get; }
        event System.Action<Sprite> onSpriteLoaded;
    }

    [RequireComponent(typeof(Image))]
    public class DUILoadingImage : DUIElement
    {
        [SerializeField]
        Image m_loadIcon;
        [SerializeField]
        Sprite m_failedSprite;
        [SerializeField]
        Sprite m_loadingSprite;
        [SerializeField]
        Sprite m_defaultSprite;
        [SerializeField]
        float m_rotationSpeed = 25f;
        [SerializeField]
        float m_angleStep = 45;
        [SerializeField]
        int m_direction = -1;
        [SerializeField]
        float m_timeOutSeconds = 60;

        RectTransform m_loadIconRect;
        Image m_image;

        float m_time;
        float m_timeLeft;
        public Image image { get { return m_image; } }
        public bool isLoading { get; private set; }
        public float timeOutSeconds { get { return m_timeOutSeconds; } set { m_timeOutSeconds = value; } } 

        public override void Init()
        {
            base.Init();
            m_loadIconRect = m_loadIcon.rectTransform;
            m_image = GetComponent<Image>();
        }

        public void SetImage(ILoadableImage loadable)
        {
            if (m_initialized == false) Init();
            if (loadable.sprite == null)
            {
                loadable.onSpriteLoaded += OnLoad;
                StartLoading();
            }
            else
            {
                OnLoad(loadable.sprite);
            }
        }

        public void SetImage(Sprite sprite)
        {
            if (m_initialized == false) Init();
            
            OnLoad(sprite);
            
        }

        public System.Action<Sprite> OnImageLoaded()
        {
            if (m_initialized == false) Init();
            StartLoading();
            return OnLoad;
        }

        public void SetDefault()
        {
            if (m_initialized == false) Init();
            m_image.sprite = m_defaultSprite;
            m_loadIcon.CrossFadeAlpha(0, 0, true);
        }

        void StartLoading()
        {
            m_loadIcon.CrossFadeAlpha(1, .5f, true);
            if(m_loadingSprite == null)
                m_image.CrossFadeAlpha(0, .5f, true);
            else m_image.sprite = m_loadingSprite;
            isLoading = true;
            m_time = 0;
            m_timeLeft = timeOutSeconds;
        }

        void OnLoad(Sprite s)
        {
            m_loadIcon.CrossFadeAlpha(0, .5f, true);
            m_image.sprite = s;
            m_image.CrossFadeAlpha(1, .5f, true);
            isLoading = false;
        }

        void Update()
        {
            if(isLoading)
            {
                m_time += Time.deltaTime * m_rotationSpeed;
                if (m_time >= 1)
                {
                    m_time = 0;
                    m_loadIconRect.Rotate(new Vector3(0, 0, m_angleStep * m_direction));
                }
                m_timeLeft -= Time.deltaTime;
                if (m_timeLeft <= 0)
                {
                    if (m_failedSprite == null)
                        m_image.CrossFadeAlpha(0, .5f, true);
                    else m_image.sprite = m_failedSprite;
                    m_loadIcon.CrossFadeAlpha(0, .5f, true);
                    isLoading = false;
                } 
            }
        }
    }

}
