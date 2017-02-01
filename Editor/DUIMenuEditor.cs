using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEditor;
namespace DynamicUI
{
    [CustomEditor(typeof(DUIMenu))]
    public class DUIDynamicListEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        //[MenuItem("CONTEXT/RectTransform/Get Path")]
        //static void GetPath(MenuCommand c)
        //{
        //    var t = c.context as RectTransform;
        //    Debug.Log(string.Format("{0}", t.GetPath(GameObject.Find("UI").transform)));
        //}
    }
}

