using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

namespace DynamicUI
{
    public class DUIMenuItemHolder : DUIItemHolder<DUIMenuItem>
    {
        public Button button;
        public Image icon;
        public Text nameText;

        public override void SetUp(DUIMenuItem item)
        {
            button.onClick.AddListener(item.OnItemPress);
            icon.sprite = item.sprite;
            nameText.text = item.name;
        }
    }

    [System.Serializable]
    public class DUIMenuItem : DUIItem
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

        public void OnItemPress()
        {
            onPress.Invoke();
        }
    }

}
