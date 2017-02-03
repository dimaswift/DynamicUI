using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DynamicUI
{
    public class DUINavigationScreen : DUIList<DUINavigationHolder>
    {
        [SerializeField]
        DUINavigationItem[] m_navItems;

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            if (m_navItems.Length > 0)
                SetItemsFromInspector();
        }

        public override void OnHolderClick(object holder)
        {
            base.OnHolderClick(holder);
            var h = holder as DUINavigationHolder;
            var n = h.item as DUINavigationItem;
            n.screen.Show();
        }

        public void SetItemsFromInspector()
        {
            SetItems(m_navItems);
        }
    }

}
