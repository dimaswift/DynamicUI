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
        float m_canvasScale;
        Vector2 m_pressedItemPos;
        Vector2 m_pressedPointerPos;

        public event System.Action onListChanged;

        List<Holder> m_backUpHolderList;
      
        public override void Init()
        {
            base.Init();
            Invoker.Add(CalculateCanvasSize, .1f);
        }

        void CalculateCanvasSize()
        {
            m_canvasScale = GetComponentInParent<CanvasScaler>().transform.localScale.x;
        }

        public override void SetItems(Item[] itemList)
        {
            base.SetItems(itemList);
           
            for (int i = 0; i < itemHolders.Count; i++)
            {
                itemHolders[i].SetParentList(this);
            }
            m_backUpHolderList = new List<Holder>(itemHolders);
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
            draggedHolder.rectTransform.SetParent(rectTransform.parent);
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
            var pointer = GetPointer();
         //   draggedItemRect.localScale = Vector3.Lerp(draggedItemRect.localScale, new Vector3(1.2f, 1.2f, 1.2f), Time.unscaledDeltaTime * 3);
            var delta = m_pressedPointerPos - pointer;
            if(m_allowDeleting == false)
                delta.x = 0;
            draggedItemRect.position = m_pressedItemPos - delta;
            var viewPortTopPos = rectTransform.position.y + (Vector3.Scale(rectTransform.sizeDelta * m_canvasScale, rectTransform.pivot)).y;
            var viewPortBottomPos = rectTransform.position.y - (Vector3.Scale(rectTransform.sizeDelta * m_canvasScale, rectTransform.pivot)).y;
            var draggedRectWidth = draggedItemRect.sizeDelta.x * m_canvasScale;
            var draggedRectHeight = draggedItemRect.sizeDelta.y * m_canvasScale;
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
            var dragItemCenterPos = draggedItemRect.position.y - (draggedRectHeight * .5f);
            var topDistanceToContainer = draggedItemRect.position.y - viewPortTopPos;
            var bottomDistanceToContainer = draggedItemRect.position.y - draggedRectHeight - viewPortBottomPos;
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
                var prevItemHeight = prevItem.rectTransform.sizeDelta.y * m_canvasScale * .5f;
                var prevItemPos = prevItem.rectTransform.position.y - (prevItemHeight);
                var distanceToPrevItem = draggedItemRect.position.y - prevItemPos;
                if (distanceToPrevItem > 0)
                {
                    draggedHolder.index = draggedIndex - 1;
                    prevItem.index = draggedIndex;
                    itemHolders[draggedIndex - 1] = draggedHolder;
                    itemHolders[draggedIndex] = prevItem;
                    SetHoldersTargetPosition();
                    OnListChanged();
                } 
            }

            if(nextItem && !nextItem.isMoving)
            {
                var nextItemHeight = nextItem.rectTransform.sizeDelta.y * m_canvasScale * .5f;
                var nextItemPos = nextItem.rectTransform.position.y - nextItemHeight;
                var distanceToNextItem = draggedItemRect.position.y - draggedRectHeight - nextItemPos;

                if (distanceToNextItem < 0)
                {
                    draggedHolder.index = draggedIndex + 1;
                    nextItem.index = draggedIndex;
                    itemHolders[draggedIndex + 1] = draggedHolder;
                    itemHolders[draggedIndex] = nextItem;
                    SetHoldersTargetPosition();
                    OnListChanged();
                }
            }
        }

        void SetHoldersTargetPosition()
        {
            float h = 0f;
            for (int i = 0; i < itemHolders.Count; i++)
            {
                var holder = itemHolders[i];
                holder.positionInList = new Vector2(0, h);
                h -= holder.rectTransform.sizeDelta.y;
            }
        }

        public override void Show()
        {
            base.Show();
            CalculateCanvasSize();
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
            draggedHolder.rectTransform.localScale = Vector3.one;
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
