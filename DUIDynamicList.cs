using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{
     public class DUIDynamicList<Holder, Item> : DUIReordableList<Holder, Item> where Holder : DUIReordableItemHolder<Item>
     {
        [SerializeField]
        Button m_addButton;
        [SerializeField]
        Button m_removeButton;

        public Holder selectedHolder { get; private set; }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_removeButton.interactable = false;
        }

        public override void Show()
        {
            base.Show();
            m_removeButton.interactable = selectedHolder != null;
        }

        public override void OnItemPointerDown(object item)
        {
            base.OnItemPointerDown(item);
            selectedHolder = item as Holder;
            m_removeButton.interactable = true;
            OnSelectItem(selectedHolder);
        }

        public virtual void OnSelectItem(Holder holder)
        {

        }

        public virtual void OnAddPressed()
        {

        }

        public virtual void OnRemovePressed()
        {

        }
    }
}
