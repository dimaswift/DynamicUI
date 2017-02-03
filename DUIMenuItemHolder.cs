using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace DynamicUI
{
    public class DUIMenuItemHolder : DUIItemHolder
    {
        public Button button;
        public Image icon;
        public Text nameText;

        public override void SetUp(object item)
        {
            base.SetUp(item);
            var menuItem = item as IMenuItem;
            button.onClick.AddListener(menuItem.OnPress);
            icon.sprite = menuItem.sprite;
            nameText.text = menuItem.name;
        }
    }

    [System.Serializable]
    public class DUIMenuItem : IMenuItem
    {
        [SerializeField]
        string m_name;
        [SerializeField]
        Sprite m_sprite;
        [SerializeField]
        UnityEvent m_onPress;

        public string name { get { return m_name; } }

        public Sprite sprite { get { return m_sprite; } }

        public UnityEvent onPress { get { return m_onPress; } }

        public void OnPress()
        {
            onPress.Invoke();
        }
    }

}
