using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    public class DUIReordableItemHolder<T> : DUIItemHolder<T>, IPointerDownHandler, IPointerUpHandler
    {
        public bool isDragging { get; private set; }

        DUIReordableListBridge m_list;

        public Vector2 positionInList { get; set; }

        public bool isMoving { get { return Mathf.Abs(positionInList.y - rectTransform.anchoredPosition.y) > 1; } }

        public bool isRedyToBeDeleted { get; set; }

        public void SetParentList(DUIReordableListBridge list)
        {
            m_list = list;
        }
        public void OnPointerDown(PointerEventData data)
        {
            m_list.OnItemPointerDown(this);
            isDragging = true;
        }

        public void OnPointerUp(PointerEventData data)
        {
            m_list.OnItemPointerUp(this);
            isDragging = false;
        }

        public virtual void OnReadyToBeDeleted(bool readyToBeDeleted)
        {

        }

        public virtual void OnUndoDelete()
        {
            gameObject.SetActive(true);
        }

        public virtual void OnDelete()
        {
            gameObject.SetActive(false);
        }

    }

}
