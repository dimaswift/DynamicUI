using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    public class DUIReordableItemHolder<T> : DUIItemHolder<T>, IPointerDownHandler, IPointerUpHandler where T : DUIItem
    {
        public bool isDragging { get; private set; }

        DUIReordableList<DUIReordableItemHolder<T>, T> m_list;

        public void Init(DUIReordableList<DUIReordableItemHolder<T>, T> list)
        {
            Init();
            m_list = list;
        }

        public void OnPointerDown(PointerEventData data)
        {
            m_list.OnItemStartDragging(this);
            isDragging = true;
        }

        public void OnPointerUp(PointerEventData data)
        {
            m_list.OnItemEndDragging(this);
            isDragging = false;
        }

        public override void SetUp(T item)
        {
            
        }
    }

}
