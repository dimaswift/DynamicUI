using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
    public abstract class DUIList<ItemHolder> : DUIElement, IDUIList where ItemHolder : DUIItemHolder
    {
        [SerializeField]
        protected ItemHolder m_itemHolderPrefab;
        [SerializeField]
        protected RectTransform m_container;
        [SerializeField]
        [HideInInspector]
        protected bool m_hasBeenSetUp;

        protected object[] m_items;
        [SerializeField]
        [HideInInspector]
        protected ScrollRect m_scroll;

        protected List<ItemHolder> m_itemHolders = new List<ItemHolder>();

        public event System.Action<ItemHolder> onItemSetUp;
        public event System.Action<ItemHolder> onHolderClicked;
        public object[] items { get { return m_items; } }
        public List<ItemHolder> itemHolders { get { return m_itemHolders; } }
        public ScrollRect scrollRect { get { return m_scroll; } }
        protected float m_canvasScale;

        void Start()
        {
            if (Application.isPlaying == false && !m_hasBeenSetUp)
            { 
                m_hasBeenSetUp = true;
                EditorSetUp();
            }
        }

        void CalculateCanvasSize()
        {
            m_canvasScale = GetComponentInParent<CanvasScaler>().transform.localScale.x;
        }


        public override void Init(DUICanvas canvas)
        {
            if (Application.isPlaying)
            {
                base.Init(canvas);
                if (m_itemHolderPrefab == null)
                    m_itemHolderPrefab = GetComponentInChildren<ItemHolder>();
                m_itemHolderPrefab.Init(this);
                Invoker.Add(CalculateCanvasSize, .1f);
            }
        }

        protected virtual void EditorSetUp()
        {
#if UNITY_EDITOR
            m_scroll = GetComponent<ScrollRect>();
            if (transform.childCount > 0)
                scrollRect.content = transform.GetChild(0).GetComponent<RectTransform>();
            m_scroll.horizontal = false;
            m_scroll.viewport = GetComponent<RectTransform>();
            m_container = m_scroll.content;
            m_itemHolderPrefab = GetComponentInChildren<ItemHolder>();
            m_hasBeenSetUp = true;
            UnityEditor.EditorUtility.SetDirty(this);
#endif  
        }

        public virtual void OnHolderClick(object holder)
        {
            if (onHolderClicked != null)
                onHolderClicked(holder as ItemHolder);
        }

        public virtual void OnItemSetUp(object holder)
        {
            if (onItemSetUp != null)
                onItemSetUp(holder as ItemHolder);
        }

        public virtual void OnItemHolderSetUp(ItemHolder holder, int index) { }

        protected virtual void SetItems(object[] itemList) 
        {
            m_itemHolderPrefab.gameObject.SetActive(false);
            m_items = itemList;
            if (itemList.Length == 0)
            {
                foreach (var item in m_itemHolders)
                {
                    item.gameObject.gameObject.SetActive(false);
                }
                CalculateCanvasSize();
                return;
            }

            float totalHeight = 0;
            float prevItemPos = 0;
            float prevItemHeight = 0;
            CalculateCanvasSize();

            for (int i = 0; i < itemList.Length; i++)
            {
                var item = itemList[i];
                var itemHolder = i < m_itemHolders.Count ? m_itemHolders[i] : Instantiate(m_itemHolderPrefab);
                itemHolder.Init(this);
                itemHolder.index = i;
                OnItemHolderSetUp(itemHolder, i);
                itemHolder.gameObject.SetActive(true);
                itemHolder.rectTransform.SetParent(m_container);
                itemHolder.rectTransform.localScale = Vector3.one;

                var customHeight = item as ICustomHeight;
                if (customHeight != null)
                {
                    itemHolder.rectTransform.sizeDelta = new Vector2(itemHolder.rectTransform.sizeDelta.x, customHeight.height);
                }
                var text = itemHolder as IText;
                if(text != null)
                {
                    var fontSize = item as IFontSize;
                    if (fontSize != null)
                    {
                        text.text.fontSize = fontSize.fontSize;
                    }
                    var textColor = item as ITextColor;
                    if (textColor != null)
                    {
                        text.text.color = textColor.textColor;
                    }
                }
                m_container.pivot = new Vector2(.5f, 1);
                m_container.anchorMax = new Vector2(.5f, 1);
                m_container.anchorMin = new Vector2(.5f, 1);
                m_container.anchoredPosition = Vector2.zero;
                
                itemHolder.rectTransform.anchorMax = new Vector2(.5f, 1);
                itemHolder.rectTransform.anchorMin = new Vector2(.5f, 1);
                itemHolder.rectTransform.pivot = new Vector2(.5f, .5f);
                prevItemPos = (prevItemPos - prevItemHeight * .5f) - (itemHolder.rectTransform.sizeDelta.y * itemHolder.rectTransform.pivot.y);
                prevItemHeight = itemHolder.rectTransform.sizeDelta.y;
                itemHolder.rectTransform.anchoredPosition = new Vector2(0, prevItemPos);
                totalHeight += itemHolder.rectTransform.sizeDelta.y;
                if (m_itemHolders.Contains(itemHolder) == false)
                    m_itemHolders.Add(itemHolder);
                itemHolder.SetUp(item);
            }

            for (int i = 0; i < m_itemHolders.Count; i++)
            {
                if (i >= itemList.Length)
                    m_itemHolders[i].gameObject.SetActive(false);
            }

            m_container.sizeDelta = new Vector2(m_container.sizeDelta.x, totalHeight);

          
        }
    }

    public interface IText
    {
        Text text { get; }
    }

    public interface ICustomHeight
    {
        float height { get; }
    }

    public interface ITextColor
    {
        Color textColor { get; }
    }

    public interface IFontSize
    {
        int fontSize { get; }
    }

}
