using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
    public class DUIReordableList<Holder, Item> : DUIList<Holder, Item> 
        where Item : DUIItem where Holder : DUIReordableItemHolder<Item>
    {
        public Holder draggedItem { get; private set; }
        public bool isDraggingItem { get; private set; }

        public override void Init()
        {
            base.Init();

        }

        public virtual void OnItemStartDragging(Holder item)
        {
            isDraggingItem = true;
            draggedItem = item;
        }

        public virtual void OnItemEndDragging(Holder item)
        {
            isDraggingItem = false;
            draggedItem = null;
        }
    }
}
