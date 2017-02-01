using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    public interface DUIReordableListBridge
    {
        void OnItemPointerDown(object holder);
        void OnItemPointerUp(object holder);
    }

    public class DUIReordableList<Holder, Item> : DUIList<Holder, Item>, DUIReordableListBridge
        where Holder : DUIReordableItemHolder<Item>
    {
        [SerializeField]
        protected float m_dragDuration = .5f;
        [SerializeField]
        protected float m_holdersMoveSpeed = 10;
        [SerializeField]
        protected bool m_allowDeleting = false;
        [SerializeField]
        float m_maxScrollSpeed = 1f;
    
        public Holder draggedHolder { get; private set; }
        public bool isDraggingItem { get; private set; }

        bool m_moveingHoldersToPos;
        bool m_isHoldingItem;
        float m_dragTimer;
   
        Vector2 m_pressedItemPos;
        Vector2 m_pressedPointerPos;

        public event System.Action onListChanged;

        List<Holder> m_backUpHolderList;

        RectTransform test;

        public override void SetItems(Item[] itemList)
        {
            test = new GameObject("Test").AddComponent<RectTransform>();
            test.SetParent(m_container);
            base.SetItems(itemList);
           
            for (int i = 0; i < itemHolders.Count; i++)
            {
                itemHolders[i].SetParentList(this);
            }
            m_backUpHolderList = new List<Holder>(itemHolders);
            SetHoldersTargetPosition();
        }

        public void UndoDeletedItems()
        {
            m_itemHolders = new List<Holder>(m_backUpHolderList);
            m_items = new Item[m_backUpHolderList.Count];
            for (int i = 0; i < m_itemHolders.Count; i++)
            {
                m_itemHolders[i].OnUndoDelete();
                m_itemHolders[i].index = i;
                m_items[i] = m_itemHolders[i].item;
            }
            SetHoldersTargetPosition();
            ResizeContainer();
        }

        protected virtual Vector2 GetPointer()
        {
            return Input.mousePosition;
        }

        protected virtual void OnStartDragging(Holder holder)
        {
            m_isHoldingItem = false;
            isDraggingItem = true;
            m_pressedItemPos = draggedHolder.rectTransform.position;
            m_pressedPointerPos = GetPointer();
            scrollRect.enabled = false;
            draggedHolder.rectTransform.SetAsLastSibling();
            SetHoldersTargetPosition();
            m_moveingHoldersToPos = true;
        }

        void OnListChanged()
        {
            for (int i = 0; i < itemHolders.Count; i++)
            {
                itemHolders[i].index = i;
                items[i] = itemHolders[i].item;
            }

            if (onListChanged != null)
                onListChanged();
        }

        protected virtual void OnDraggingItem(Holder holder)
        {
            var draggedItemRect = draggedHolder.rectTransform;
            var draggedRectHeight = draggedItemRect.sizeDelta.y * m_canvasScale;
            var pointer = GetPointer();
         //   draggedItemRect.localScale = Vector3.Lerp(draggedItemRect.localScale, new Vector3(1.2f, 1.2f, 1.2f), Time.unscaledDeltaTime * 3);
            var delta = m_pressedPointerPos - pointer;
            if(m_allowDeleting == false)
                delta.x = 0;
            draggedItemRect.position = m_pressedItemPos - delta;
            var draggedPos = draggedItemRect.anchoredPosition.y + (draggedItemRect.sizeDelta.y * m_canvasScale * draggedItemRect.pivot.y);
            var viewPortTopPos = rectTransform.position.y + (Vector3.Scale(rectTransform.sizeDelta * m_canvasScale, rectTransform.pivot)).y;
            var viewPortBottomPos = rectTransform.position.y - (Vector3.Scale(rectTransform.sizeDelta * m_canvasScale, rectTransform.pivot)).y;
            var draggedRectWidth = draggedItemRect.sizeDelta.x * m_canvasScale;
       
            if (m_allowDeleting)
            {
                bool readyToBeDeleted = Mathf.Abs(draggedItemRect.anchoredPosition.x) > draggedRectWidth * .5f;
                if(readyToBeDeleted != draggedHolder.isRedyToBeDeleted)
                {
                    draggedHolder.isRedyToBeDeleted = readyToBeDeleted;
                    draggedHolder.OnReadyToBeDeleted(readyToBeDeleted);
                }
            }
            var draggedIndex = draggedHolder.index;

            var prevItem = draggedIndex > 0 ? itemHolders[draggedIndex - 1]  : null;
            var nextItem = draggedIndex < itemHolders.Count - 1 ? itemHolders[draggedIndex + 1] : null;

            var topDistanceToContainer = (draggedItemRect.position.y - viewPortTopPos) + (draggedRectHeight * .5f);
            var bottomDistanceToContainer = (draggedItemRect.position.y - viewPortBottomPos) - (draggedRectHeight * .5f);



            if (topDistanceToContainer > 0)
            {
                var scrollSpeed = Helper.Remap(topDistanceToContainer, 0, draggedRectHeight, 0, m_maxScrollSpeed * .5f);
                if (scrollRect.normalizedPosition.y < 1)
                {
                    scrollRect.normalizedPosition += Vector2.up * Time.unscaledDeltaTime * scrollSpeed;
                }
            }
            if (bottomDistanceToContainer < 0)
            {
                var scrollSpeed = Helper.Remap(bottomDistanceToContainer, 0, -draggedRectHeight, 0, m_maxScrollSpeed * .5f);
                if (scrollRect.normalizedPosition.y > 0)
                {
                    scrollRect.normalizedPosition -= Vector2.up * Time.unscaledDeltaTime * scrollSpeed;
                }
            }

            if (prevItem && !prevItem.isMoving)
            {
                var prevItemHeight = prevItem.rectTransform.sizeDelta.y * m_canvasScale;
                var prevItemPos = prevItem.rectTransform.anchoredPosition.y - (prevItemHeight * prevItem.rectTransform.pivot.y);
                var distanceToPrevItem = draggedPos - prevItemPos;
                var swapThreshold = prevItemHeight * .25f;
                if (distanceToPrevItem > (draggedRectHeight * .5f) + swapThreshold)
                {
                    draggedHolder.index = draggedIndex - 1;
                    prevItem.index = draggedIndex;
                    itemHolders[draggedIndex - 1] = draggedHolder;
                    itemHolders[draggedIndex] = prevItem;
                    SetHoldersTargetPosition();
                }
            }
            if (nextItem && !nextItem.isMoving)
            {
                var nextItemHeight = nextItem.rectTransform.sizeDelta.y * m_canvasScale;
                var nextItemPos = nextItem.rectTransform.anchoredPosition.y + (nextItemHeight * nextItem.rectTransform.pivot.y);
                var distanceToNextItem = draggedPos - nextItemPos;
                var swapThreshold = nextItemHeight * .25f;
                if (distanceToNextItem < (draggedRectHeight * .5f) - swapThreshold)
                {
                    draggedHolder.index = draggedIndex + 1;
                    nextItem.index = draggedIndex;
                    itemHolders[draggedIndex + 1] = draggedHolder;
                    itemHolders[draggedIndex] = nextItem;
                    SetHoldersTargetPosition();
                }
            }
        }

        void SetHoldersTargetPosition()
        {
            var h = 0f;
            var p = 0f;
            for (int i = 0; i < itemHolders.Count; i++)
            {
                var holder = itemHolders[i];
                var newH = holder.rectTransform.sizeDelta.y;
                p = (p - h * .5f) - (newH * holder.rectTransform.pivot.y);
                h = newH;
                holder.positionInList = new Vector2(0, p);
            }
        }

        void MoveHolderToPosition()
        {
            if (m_moveingHoldersToPos == false) return;
            bool hasMovingHolders = false;
            for (int i = 0; i < itemHolders.Count; i++)
            {
                var holder = itemHolders[i];
                if (holder == draggedHolder)
                    continue;
                if (!hasMovingHolders)
                {
                    var d = holder.rectTransform.anchoredPosition.y - holder.positionInList.y;
                    if (d != 0)
                        hasMovingHolders = true;
                }
                holder.rectTransform.anchoredPosition = Vector2.Lerp(holder.rectTransform.anchoredPosition, holder.positionInList, Time.deltaTime * 10);
            }
            if (hasMovingHolders == false && isDraggingItem == false)
                m_moveingHoldersToPos = false;

        }

        protected virtual void OnItemDeleted(Holder holder) { }

        void DeleteItem(Holder holder)
        {
            holder.OnDelete();
            itemHolders.Remove(holder);
            System.Array.Resize(ref m_items, items.Length - 1);
            OnListChanged();
            OnItemDeleted(holder);
            ResizeContainer();
        }

        void ResizeContainer()
        {
            var h = 0f;
            for (int i = 0; i < itemHolders.Count; i++)
            {
                h += itemHolders[i].rectTransform.sizeDelta.y;
            }
            m_container.sizeDelta = new Vector2(m_container.sizeDelta.x, h);
        }

        void OnDraggingEnd()
        {
            if(m_allowDeleting)
            {
                if(draggedHolder.isRedyToBeDeleted)
                {
                    DeleteItem(draggedHolder);
                }
                draggedHolder.isRedyToBeDeleted = false;
                draggedHolder.OnReadyToBeDeleted(false);
            }
            
            isDraggingItem = false;
            scrollRect.enabled = true;
            draggedHolder.rectTransform.SetParent(m_container);
            draggedHolder = null;
            SetHoldersTargetPosition();
            
        }

        void Update()
        {
            if(m_isHoldingItem)
            {
                m_dragTimer += Time.unscaledDeltaTime;
                if(m_dragTimer > m_dragDuration)
                {
                    OnStartDragging(draggedHolder);
                }
            }
            if(isDraggingItem)
            {
                OnDraggingItem(draggedHolder);
                if(Input.GetMouseButtonUp(0))
                {
                    OnDraggingEnd();
                }
            }
            if (Input.GetKeyDown(KeyCode.Z))
                UndoDeletedItems();
            MoveHolderToPosition();
        }

        public virtual void OnItemPointerDown(object item)
        {
            if(isDraggingItem == false)
            {
                m_isHoldingItem = true;
                m_dragTimer = 0;
                draggedHolder = item as Holder;
            }
        }

        public virtual void OnItemPointerUp(object item)
        {
            if (isDraggingItem == false)
            {
                m_isHoldingItem = false;
                draggedHolder = null;
            }
        }
    }
}
