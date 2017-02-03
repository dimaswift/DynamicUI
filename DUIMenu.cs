using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace DynamicUI
{
    public interface IMenuItem
    {
        string name { get; }

        Sprite sprite { get; }

        void OnPress();
    }

    public class DUIMenu : DUIList<DUIMenuItemHolder>
    {
        [SerializeField]
        DUIMenuItem[] m_menuItems;

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            if (m_menuItems.Length > 0)
                SetMenuItemsFromInspector();
        }

        public void SetMenuItemsFromInspector()
        {
            SetItems(m_menuItems);
        }

        public void SetMenuItems(IMenuItem[] items)
        {
            SetItems(items);
        }

    }

}
