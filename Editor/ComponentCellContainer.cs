using UnityEngine;
using System.Collections.Generic;
using HandyUtilities;
using CodeGenerator;
namespace DynamicUI
{
    public class ComponentCellContainer : SerializedSingleton<ComponentCellContainer>
    {
        public bool pendingScriptCompile;
        public string newTypeName;
        public int panelID;
        public string elementsClass;
        public List<ComponentCell> cells = new List<ComponentCell>();

    }
}
