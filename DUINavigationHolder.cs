using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    [RequireComponent(typeof(Image))]
     public class DUINavigationHolder : DUIItemHolder, IPointerClickHandler
     {
        public Image iconImage;
        public Text nameText;

        public override void SetUp(object item)
        {
            base.SetUp(item);
            var navItem = item as DUINavigationItem;
            iconImage.sprite = navItem.icon;
            nameText.text = Local.Translate(navItem.screenName);
        }
    }

    [System.Serializable]
    public class DUINavigationItem
    {
        public Sprite icon;
        public string screenName;
        public DUIScreen screen;
    }
}
