using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

namespace DynamicUI
{
    [System.Serializable]
    public class DUIPendingComponentContainer
    {
        public string typeName;
        public int targetGameObjectID;
    }

    public class DUIFactory : Editor
    {
        static string DUIFolder { get { return HandyEditor.FindFolderInProject("DynamicUI"); } }


        [MenuItem("GameObject/DynamicUI/List")]
        static void OnCreateListPressed()
        {
            ConfirmationTool.OpenWithArguments("Create DUIList", "Create", (args) =>
            {
                CreateList(
                    args[0] as string,
                    args[1] as string,
                    args[2] as string,
                    args[3] as string);
            }, 
            new ConfirmationTool.Label("List Class Name", "MyList"),
            new ConfirmationTool.Label("Holder Class Name", "MyListHolder"),
            new ConfirmationTool.Label("Item Class Name", "MyListItem"),
            new ConfirmationTool.Label("Namespace"));
        }


        public static void CreateList(string listName, string holderName, string itemName, string nameSpace)
        {
            var duiFolder = DUIFolder;

            var listPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DUIFolder + "/Prefabs/List.prefab");

            string listScriptString = "";
            #region List Script String

            if (string.IsNullOrEmpty(nameSpace))
            {
                listScriptString = string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;

public class {0} : DUIList<{1}, {2}>
{{
	
}}
", listName, holderName, itemName);
            }
            else
            {
                listScriptString = string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;

namespace {3}
{{
    public class {0} : DUIList<{1}, {2}>
    {{

    }}
}}
", listName, holderName, itemName, nameSpace);
            }
            #endregion List Script String

            string holdertString = "";
            #region Holder String

            if (string.IsNullOrEmpty(nameSpace))
            {
                holdertString = string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;
using UnityEngine.UI;

public class {0} : DUIItemHolder<{1}>
{{
    
}}

[System.Serializable]
public class {1}
{{

}}", holderName, itemName);
            }
            else
            {
                holdertString = string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;
using UnityEngine.UI;
namespace {2}
{{
    public class {0} : DUIItemHolder<{1}>
    {{
    
    }}

    [System.Serializable]
    public class {1}
    {{

    }}
}}", holderName, itemName, nameSpace);

            }
            #endregion Holder String

            var dir = Helper.ConvertToAbsolutePath("Assets/Scripts");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string listScriptPath = dir + "/" + listName + ".cs";

            string holderScriptPath = dir + "/" + holderName + ".cs";

            if(File.Exists(listScriptPath) || File.Exists(holderScriptPath))
            {
                if (!EditorUtility.DisplayDialog("Warning", "Script files already exist. Overwrite?", "Yes", "Cancel"))
                    return;
            }

            File.WriteAllText(listScriptPath, listScriptString);
            File.WriteAllText(holderScriptPath, holdertString);

            var canvas = FindObjectOfType<Canvas>();

            var listObject = Instantiate(listPrefab);
            listObject.name = listName;
          
            if (canvas)
                listObject.transform.SetParent(canvas.transform);

            var holderObject = listObject.transform.FindChild("container/holder");

            var container = listObject.transform.FindChild("container");

            listObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            var listType = string.IsNullOrEmpty(nameSpace) ? listName : nameSpace + "." + listName;
            var holderType = string.IsNullOrEmpty(nameSpace) ? holderName : nameSpace + "." + holderName;
            DUISettings.Instance.pendingComponents.Add(new DUIPendingComponentContainer() { targetGameObjectID = holderObject.gameObject.GetInstanceID(), typeName = holderType });
            DUISettings.Instance.pendingComponents.Add(new DUIPendingComponentContainer() { targetGameObjectID = listObject.gameObject.GetInstanceID(), typeName = listType });
          
            EditorUtility.SetDirty(DUISettings.Instance);

            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(listScriptPath));
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(holderScriptPath));
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var settings = DUISettings.Instance;
            foreach (var binding in settings.pendingComponents)
            {
                var gameObject = EditorUtility.InstanceIDToObject(binding.targetGameObjectID) as GameObject;
                if(gameObject)
                {
                    var type = Helper.GetType(binding.typeName);
                    if (type != null)
                    {
                        gameObject.AddComponent(type);
                    }
                }
            }
            settings.pendingComponents.Clear();
            EditorUtility.SetDirty(settings);
        }

    }

 

}
