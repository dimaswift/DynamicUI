using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    public interface ISelectable
    {
        bool selected { get; }
    }

    public class DUIInfiniteList<ItemHolder> : DUIList<ItemHolder>, IDUIList
        where ItemHolder : DUIItemHolder
    {

        protected int m_cellIndex;
        Vector2 m_prevScrollPos;

        int m_cellCount;

        public override void Init(DUICanvas canvas)
        {
            if (m_initialized) return;
            m_itemHolderPrefab.Init(this);
            base.Init(canvas);
            scrollRect.onValueChanged.AddListener(OnScroll);
        
            Invoke("CalculateSizes", .1f);
        }

        protected override void EditorSetUp()
        {
            GetComponent<RectTransform>().pivot = new Vector2(.5f, 1f);
            base.EditorSetUp();
        }

        public void SetItems<T>(T[] itemList) where T : class, ISelectable
        {
            base.SetItems(itemList);
            var totalHeight = 0f;
            var holderHeight = m_itemHolderPrefab.rectTransform.sizeDelta.y;
            m_cellCount = (int) (m_container.sizeDelta.y / holderHeight) + 4;
            if (itemList.Length < m_cellCount)
                m_cellCount = itemList.Length;

            for (int i = 0; i < m_cellCount; i++)
            {
                var item = itemList[i];
                var itemHolder = i > itemHolders.Count - 1 ? Instantiate(m_itemHolderPrefab) : itemHolders[i];
                itemHolder.Init(this);
                if (itemHolder.selectable)
                    itemHolder.selectable.SetSelectedImmediately(item.selected);
                itemHolder.index = i;
                itemHolder.SetUp(item);
                OnItemHolderSetUp(itemHolder, i);
                itemHolder.gameObject.SetActive(true);
                itemHolder.rectTransform.SetParent(m_container);
                itemHolder.rectTransform.localScale = Vector3.one;
                m_container.pivot = new Vector2(.5f, 1);
                m_container.anchorMax = new Vector2(.5f, 1);
                m_container.anchorMin = new Vector2(.5f, 1);
                m_container.anchoredPosition = Vector2.zero;
                itemHolder.rectTransform.pivot = new Vector2(.5f, 1f);
                itemHolder.rectTransform.anchorMax = new Vector2(.5f, 1);
                itemHolder.rectTransform.anchorMin = new Vector2(.5f, 1);
                itemHolder.rectTransform.anchoredPosition = new Vector2(0, -totalHeight);
                totalHeight += itemHolder.rectTransform.sizeDelta.y;
                if (itemHolders.Contains(itemHolder) == false)
                    itemHolders.Add(itemHolder);
            }

            for (int i = 0; i < itemHolders.Count; i++)
            {
                if (i >= itemList.Length)
                    itemHolders[i].gameObject.SetActive(false);
            }

            m_container.sizeDelta = new Vector2(m_container.sizeDelta.x, itemList.Length * holderHeight);

            m_itemHolderPrefab.gameObject.SetActive(false);
            
        }

        void CalculateSizes()
        {
            m_canvasScale = GetComponentInParent<CanvasScaler>().transform.localScale.x;
        }

        protected void OnScroll(Vector2 position)
        {
            Vector2 m_currentScrollPos = m_container.position;
            var viewportPositionY = rectTransform.position.y + (Vector3.Scale(rectTransform.sizeDelta, rectTransform.pivot)).y;
            var viewportHeight = rectTransform.sizeDelta.y;
            var cellHeight = m_itemHolderPrefab.rectTransform.sizeDelta.y;
            var firstCell = itemHolders[0];
            var lastCell = itemHolders.LastItem();

            var delta = m_currentScrollPos - m_prevScrollPos;
            if (delta.y > 0)
            {
                var diff = (firstCell.rectTransform.position.y - viewportPositionY) / m_canvasScale;
                var prev = diff;
                while (diff > cellHeight)
                {
                    if (m_cellIndex + m_cellCount < items.Length)
                    {
                        m_cellIndex++;
                        firstCell.rectTransform.anchoredPosition = lastCell.rectTransform.anchoredPosition - (Vector2.up * cellHeight);
                        firstCell.SetUp(items[m_cellCount + m_cellIndex - 1]);
                        if (firstCell.selectable)
                            firstCell.selectable.SetSelectedImmediately(((ISelectable) firstCell.item).selected);
                        itemHolders.RemoveAt(0);
                        itemHolders.Add(firstCell);
                    }
                    firstCell = itemHolders[0];
                    lastCell = itemHolders.LastItem();
                    diff = (firstCell.rectTransform.position.y - viewportPositionY) / m_canvasScale;
                    if (prev == diff)
                        break;
                    prev = diff;
                }
            }
            else if (delta.y < 0)
            {
                var diff = (lastCell.rectTransform.position.y - viewportPositionY) / m_canvasScale;
                diff += viewportHeight;
                var prev = diff;
                while (diff < -cellHeight)
                {
                    if (m_cellIndex > 0)
                    {
                        m_cellIndex--;
                        lastCell.rectTransform.anchoredPosition = firstCell.rectTransform.anchoredPosition + (Vector2.up * cellHeight);
                        lastCell.SetUp(items[m_cellIndex]);
                        if (lastCell.selectable)
                            lastCell.selectable.SetSelectedImmediately(((ISelectable) lastCell.item).selected);
                        itemHolders.RemoveAt(m_cellCount - 1);
                        itemHolders.Insert(0, lastCell);
                    }
                    firstCell = itemHolders[0];
                    lastCell = itemHolders.LastItem();
                    diff = (lastCell.rectTransform.position.y - viewportPositionY) / m_canvasScale;
                    diff += viewportHeight;
                    if (prev == diff)
                        break;
                    prev = diff;
                }
            }
            m_prevScrollPos = m_currentScrollPos;
        }
    }

    public class DUIInfiniteListItem
    {
        public bool selected;
    }
}
