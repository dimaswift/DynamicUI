using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    }
}

