using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    public class DUIReordableItemHolder : DUIItemHolder, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public bool isDragging { get; private set; }

        IDUIListExtended m_list;

        public Vector2 positionInList { get; set; }

        public bool isMoving { get { return Mathf.Abs(positionInList.y - rectTransform.anchoredPosition.y) > 1; } }

        public bool isRedyToBeDeleted { get; set; }

        public void SetParentList(IDUIListExtended list)
        {
            m_list = list;
        }

        public virtual void OnPointerDown(PointerEventData data)
        {
            m_list.OnHolderPointerDown(this);
            isDragging = true;
        }

        public virtual void OnPointerUp(PointerEventData data)
        {
            m_list.OnHolderPointerUp(this);
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
