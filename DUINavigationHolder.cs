using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DynamicUI
{
    [RequireComponent(typeof(Image))]
     public class DUINavigationHolder : DUIItemHolder<DUINavigationItem>, IPointerClickHandler
     {
        public Image iconImage;
        public Text nameText;

        public override void SetUp(DUINavigationItem item)
        {
            base.SetUp(item);
            iconImage.sprite = item.icon;
            nameText.text = item.screenName;
        }

        public void OnPointerClick(PointerEventData data)
        {
            item.screen.Show();
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
