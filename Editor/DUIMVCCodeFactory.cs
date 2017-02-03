using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HandyUtilities;

namespace DynamicUI
{
    public class DUIMVCCodeFactory : Editor
    {
        [MenuItem("Dynamic UI/MVC/Create View")]
        static void CreateView()
        {
            CreateScript("View", @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;
using UnityEngine.UI;

namespace {0}.View
{{
    public class {1} : DUIScreen
    {{
        
    }}
}}");
        }

        [MenuItem("Dynamic UI/MVC/Create Controller")]
        static void CreateController()
        {
            CreateScript("Controller", @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;
using UnityEngine.UI;
using {0}.Model;
using {0}.View;

namespace {0}.Controller
{{
    public class {1}
    {{
        
    }}
}}");

        }

        [MenuItem("Dynamic UI/MVC/Create Model")]
        static void CreateModel()
        {
            CreateScript("Model", @"using System.Collections;
using System.Collections.Generic;

namespace {0}.Model
{{
    public class {1}
    {{
        
    }}
}}");

        }

        static void CreateScript(string type, string content)
        {
            ConfirmationTool.OpenWithArguments("Create MVC "+ type + " Script", "Create", (args) =>
            {
                var nmspace = args[0] as string;
                var name = args[1] as string;
                var path = EditorUtility.SaveFilePanelInProject("Choose folder", name, "cs", "", "Assets/" + DUISettings.Instance.UIRootFolder);
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(nmspace))
                {
                    string script = "";
                    script = string.Format(content, nmspace, name);
                    System.IO.File.WriteAllText(path, script);
                    AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(path));
                    AssetDatabase.Refresh();
                }
            },
                new ConfirmationTool.Label("Namespace", DUISettings.Instance.Namespace),
                new ConfirmationTool.Label("Name", "MyController"));
        }

    }

}
