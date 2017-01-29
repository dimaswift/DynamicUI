using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
    public abstract class DUIList<ItemHolder, Item> : DUIElement where ItemHolder : DUIItemHolder<Item>
    {
        [SerializeField]
        protected ItemHolder m_itemHolderPrefab;
        [SerializeField]
        protected RectTransform m_container;
        [SerializeField]
        [HideInInspector]
        protected bool m_hasBeenSetUp;
        [SerializeField]
        protected Item[] m_items;
        [SerializeField]
        [HideInInspector]
        protected ScrollRect m_scroll;

        protected List<ItemHolder> m_itemHolders = new List<ItemHolder>();

        public Item[] items { get { return m_items; } }
        public List<ItemHolder> itemHolders { get { return m_itemHolders; } }
        public ScrollRect scrollRect { get { return m_scroll; } }


        void Start()
        {
            if (Application.isPlaying == false && !m_hasBeenSetUp)
            { 
                m_hasBeenSetUp = true;
                EditorSetUp();
            }
        }

        public override void Init()
        {
            if (Application.isPlaying)
            {
                base.Init();
                SetItems(m_items);
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

        public virtual void OnItemHolderSetUp(ItemHolder holder, int index) { }

        public virtual void SetItems(Item[] itemList)
        {
            float totalHeight = 0;
            m_items = itemList;
            for (int i = 0; i < itemList.Length; i++)
            {
                var item = itemList[i];
                var itemHolder = i < m_itemHolders.Count ? m_itemHolders[i] : Instantiate(m_itemHolderPrefab);
                itemHolder.Init();
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
                itemHolder.rectTransform.pivot = new Vector2(.5f, 1f);
                itemHolder.rectTransform.anchorMax = new Vector2(.5f, 1);
                itemHolder.rectTransform.anchorMin = new Vector2(.5f, 1);
                itemHolder.rectTransform.anchoredPosition = new Vector2(0, -totalHeight);
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

            m_itemHolderPrefab.gameObject.SetActive(false);
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
