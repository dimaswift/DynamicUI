using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HandyUtilities;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class DUIItemHolder<T> : MonoBehaviour
    {
        public RectTransform rectTransform { get; private set; }

        public int index { get; set; }

        public T item { get; private set; }

        public virtual void Init()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void SetUp(T item)
        {
            this.item = item;
        }
        
    }

}
