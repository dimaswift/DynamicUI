using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HandyUtilities;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class DUIItemHolder : MonoBehaviour, IPointerClickHandler
    {
        public RectTransform rectTransform { get; private set; }

        public int index { get; set; }

        public object item { get; private set; }

        public DUISelectable selectable { get; private set; }

        public IDUIList list { get; private set; }

        public virtual void Init(IDUIList list)
        {
            this.list = list;
            rectTransform = GetComponent<RectTransform>();
            selectable = GetComponent<DUISelectable>();
        }

        public virtual void SetUp(object item)
        {
            this.item = item;
            list.OnItemSetUp(this);
        }

        public void OnPointerClick(PointerEventData data)
        {
            list.OnHolderClick(this);
        }
    }

}
