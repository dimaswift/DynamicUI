using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DynamicUI
{
    [System.Flags]
    public enum SelectionMethod
    {
        Color = 1, SpriteSwap = 2, OverlayObject = 4, Scale = 8
    }

    public class DUISelectable : MonoBehaviour, IPointerClickHandler
     {
        [SerializeField]
        SelectionSettings m_selectionSettings;
        [SerializeField]
        bool m_selectedByDefault = false;


        [SerializeField]
        Image m_selectionImage;

        Sprite m_defaultSprite;
        Vector3 m_defaultScale;

        public bool isSelected { get; private set; }
        [System.NonSerialized]
        bool m_initialized;

        int m_scalingRoutineID;

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (m_initialized) return;
            m_defaultSprite = m_selectionImage.sprite;
            m_defaultScale = m_selectionImage.rectTransform.localScale;
            isSelected = m_selectedByDefault;
            m_initialized = true;
            SetSelectedImmediately(isSelected);
        }

        void ScaleTowards(float n)
        {
            var from = !isSelected ? m_selectionSettings.selectedScale : m_defaultScale;
            var to = isSelected ? m_selectionSettings.selectedScale : m_defaultScale;
            m_selectionImage.rectTransform.localScale = Vector3.Lerp(from, to, Mathf.Lerp(0,1,n));
        }

        public void Toggle()
        {
            isSelected = !isSelected;
            Select(isSelected);
        }

        public void OnPointerClick(PointerEventData data)
        {
            Toggle();
        }

        bool HasMethod(SelectionMethod m)
        {
            return (m_selectionSettings.method & m) == m;
        }

        public virtual void SetSelectedImmediately(bool select)
        {
            Init();
            isSelected = select;
            var s = m_selectionSettings;

            if (HasMethod(SelectionMethod.SpriteSwap))
            {
                m_selectionImage.sprite = select ? s.selectedSprite : m_defaultSprite;
            }
            if (HasMethod(SelectionMethod.OverlayObject))
            {
               s.overlayImage.CrossFadeAlpha(select ? 1 : 0, 0, true);
            }
            if (HasMethod(SelectionMethod.Color))
            {
                var targetColor = select ? m_selectionSettings.selectedColor : m_selectionSettings.basicColor;
                m_selectionImage.CrossFadeColor(targetColor, 0, true, true);
            }
            if (HasMethod(SelectionMethod.Scale))
            {
                m_selectionImage.rectTransform.localScale = select ? s.selectedScale : m_defaultScale;
            }
        }

        public virtual void Select(bool select)
        {
            Init();
            isSelected = select;
          
            var s = m_selectionSettings;
           
            if (HasMethod(SelectionMethod.SpriteSwap))
            {
                m_selectionImage.sprite = select ? s.selectedSprite : m_defaultSprite;
            }
            if (HasMethod(SelectionMethod.OverlayObject))
            {
                if (s.animatedTransition)
                    s.overlayImage.CrossFadeAlpha(select ? 1 : 0, s.transitionDuration, true);
            }
            if (HasMethod(SelectionMethod.Color))
            {
                var targetColor = select ? m_selectionSettings.selectedColor : m_selectionSettings.basicColor;
                if (s.animatedTransition)
                    m_selectionImage.CrossFadeColor(targetColor, s.transitionDuration, true, true);
                else m_selectionImage.color = targetColor;
            }
            if (HasMethod(SelectionMethod.Scale))
            {
                if(s.animatedTransition)
                {
                    Invoker.StartRoutine(ScaleTowards, m_selectionSettings.transitionDuration);
                }
                else m_selectionImage.rectTransform.localScale = select ? s.selectedScale : m_defaultScale;
            }

        }

        [System.Serializable]
        public class SelectionSettings
        {
            [EnumFlag]
            public SelectionMethod method;
            public Color selectedColor = Color.green, basicColor = Color.white;
            public Sprite selectedSprite;
            public Image overlayImage;
            public bool animatedTransition = false;
            public float transitionDuration = .5f;
            public Vector3 selectedScale = new Vector3(1.1f, 1.1f, 1.1f);
        }
    }
}
