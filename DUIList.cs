using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
    public abstract class DUIList<ItemHolder, Item> : DUIElement where ItemHolder : DUIItemHolder<Item> where Item : DUIItem
    {
        [SerializeField]
        ItemHolder m_itemHolderPrefab;
        [SerializeField]
        RectTransform m_container;
        [SerializeField]
        [HideInInspector]
        bool m_hasBeenSetUp;
        [SerializeField]
        Item[] m_items;

        List<ItemHolder> m_itemHolders = new List<ItemHolder>();

        public List<ItemHolder> itemHolders { get { return m_itemHolders; } }

        void Awake()
        {
            if (Application.isEditor)
            { 
                m_hasBeenSetUp = true;
                SetUp();
            }
        }

        public override void Init()
        {
            base.Init();
            SetItems(m_items);
        }

        public void SetUp()
        {
            var scroll = GetComponent<ScrollRect>();
            if (transform.childCount > 0)
                scroll.content = transform.GetChild(0).GetComponent<RectTransform>();
            scroll.horizontal = false;
            scroll.viewport = GetComponent<RectTransform>();
            m_container = scroll.content;
            m_itemHolderPrefab = GetComponentInChildren<ItemHolder>();
            m_hasBeenSetUp = true;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            
        }

        public virtual void OnItemHolderSetUp(ItemHolder holder, int index) { }

        public virtual void SetItems(Item[] itemList)
        {
            float totalHeight = 0;
            for (int i = 0; i < itemList.Length; i++)
            {
                var item = itemList[i];
                var itemHolder = i < m_itemHolders.Count ? m_itemHolders[i] : Instantiate(m_itemHolderPrefab);
                itemHolder.Init();
                itemHolder.SetUp(item);
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
