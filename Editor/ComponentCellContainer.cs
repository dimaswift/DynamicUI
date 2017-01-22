using UnityEngine;
using System.Collections.Generic;
using HandyUtilities;
using CodeGenerator;
namespace DynamicUI
{
    public class ComponentCellContainer : SerializedSingleton<ComponentCellContainer>
    {
        public bool pendingScriptCompile;

        public List<ScreenBinding> screenBindings = new List<ScreenBinding>();

        public string newTypeName;
        public int panelID;
        public string elementsClassString;
        public List<ComponentCell> cells = new List<ComponentCell>();
        public Class pendingClass;
        public OperationType operationType;
        public enum OperationType { Create, Update }

        [System.Serializable]
        public class ScreenBinding
        {
            public string screenName;
            public string fieldName;
            public int panelID;
        }
    }
}
