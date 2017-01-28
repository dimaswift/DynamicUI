using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEditor;

namespace DynamicUI
{
    public class DUISettings : SerializedSingleton<DUISettings>
    {
        public string UIManagerClassName = "UIManager";
        public string UIRootFolder = "Scripts/";
        public string Namespace = "";

        public bool configured = false;
        [HideInInspector]
        public bool pendingAddCanvasScript = false;

        [HideInInspector]
        public List<DUIPendingComponentContainer> pendingComponents = new List<DUIPendingComponentContainer>();
    }

}
