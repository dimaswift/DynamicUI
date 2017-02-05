using UnityEngine;
using UnityEditor;
using HandyUtilities;
using System.IO;
using CodeGenerator;

namespace SwiftMVC
{
    [CustomEditor(typeof(MVCProjectSettings))]
    public class MVCScriptFactory : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var settings = target as MVCProjectSettings;
            if (GUILayout.Button("Create Project Structure"))
            {
                var appString = string.Format(GetAppString(), settings.nameSpace);
                var vewsDelegate = GetViewsDelegateClass();
                var controllersDelegate = GetControllersDelegateClass();
                var absoluteFolderPath = Helper.ConvertToAbsolutePath(settings.projectFolder);
                if (Directory.Exists(absoluteFolderPath) == false)
                    Directory.CreateDirectory(absoluteFolderPath);

                Directory.CreateDirectory(absoluteFolderPath + "/Controllers");
                Directory.CreateDirectory(absoluteFolderPath + "/Views");
                Directory.CreateDirectory(absoluteFolderPath + "/Model");

                WriteControllersDelegateClass(controllersDelegate);
                WriteViewsDelegateClass(vewsDelegate);
                File.WriteAllText(absoluteFolderPath + "/App.cs", appString);
                File.WriteAllText(absoluteFolderPath + "/Model/DummyData.cs", string.Format(GetDummyModelScript(), settings.nameSpace));
                AssetDatabase.Refresh();
            }
        }

        static string GetDummyModelScript()
        {
            return @"
namespace {0}.Model
{{
    public class {0}Data
    {{
        public string dataExample = ""Hello World"";
    }}
}}
";
        }

        static string GetAppString()
        {
            return @"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;

using {0}.Model;
using {0}.View;
using {0}.Controller;

namespace {0}
{{
    public class App : MonoBehaviour
    {{
        static App m_isntance;

        ViewsDelegate m_viewsDelegate;

        ControllersDelegate m_controllersDelegate;

        public static App Instance {{ get {{ return m_isntance; }} }}

        public static ViewsDelegate Views {{ get {{ return m_isntance.m_viewsDelegate; }} }}

        public static ControllersDelegate Controllers {{ get {{ return m_isntance.m_controllersDelegate; }} }}

        void Start()
        {{
            m_isntance = this;
            Init();
        }}

        void Init()
        {{
            m_viewsDelegate = FindObjectOfType<ViewsDelegate>();
            if(m_viewsDelegate == null) 
            {{
                Debug.LogError(""ViewsDelegate Object not found in the scene. Attach ViewsDelegate component to the root Canvas!""); 
                return;
            }}
            m_viewsDelegate.Init();
            m_viewsDelegate.HideAll();
            m_controllersDelegate = new ControllersDelegate();
            m_controllersDelegate.Init();
        }}
    }}
}}
";
        }

        [MenuItem("CONTEXT/RectTransform/Create View-Controller Pair")]
        static void CreateViewCommand(MenuCommand c)
        {
            var target = c.context as RectTransform;
            CreateViewControllerPair(target.name, target.gameObject);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Add Controller")]
        static void CreateControllerCommand(MenuCommand c)
        {
            var target = c.context as MonoBehaviour;
            AddController(target);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Add View")]
        static void AddViewReferenceCommand(MenuCommand c)
        {
            var target = c.context as MonoBehaviour;
            AddView(target);
        }


        [MenuItem("CONTEXT/RectTransform/Remove View")]
        static void RemoveViewCommand(MenuCommand c)
        {
            if (!EditorUtility.DisplayDialog("Warning", "Are you sure you want to remove all scripts and references of selected view?", "Yes", "Cancel"))
                return;
            var settings = MVCProjectSettings.Instance;
            var target = c.context as RectTransform;

            var projectPath = Helper.ConvertToAbsolutePath(MVCProjectSettings.Instance.projectFolder);
            var controller = projectPath + "/Controllers/" + target.name + "Controller.cs";
            File.Delete(controller);
            var view = projectPath + "/Views/" + target.name + "View.cs";
            File.Delete(view);

            Class viewDelegateClass = GetViewsDelegateClass();
            Class controllerDelegateClass = GetControllersDelegateClass();


            viewDelegateClass.members.RemoveAll(m => m.type.Contains(target.name + "View"));
            controllerDelegateClass.members.RemoveAll(m => m.type.Contains(target.name + "Controller"));

            foreach (var m in viewDelegateClass.members)
            {
                var method = m as Method;
                if (method != null)
                {
                    method.lines.RemoveAll(l => l.Contains(target.name.ToLower()));
                }
            }

            foreach (var m in controllerDelegateClass.members)
            {
                var method = m as Method;
                if (method != null)
                {
                    method.lines.RemoveAll(l => l.Contains(target.name.ToLower()));
                }
            }

            WriteControllersDelegateClass(controllerDelegateClass);
            WriteViewsDelegateClass(viewDelegateClass);
  
            AssetDatabase.Refresh();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnScriptsCompiled()
        {
            var settings = MVCProjectSettings.Instance;
            if(settings.isPendingViewControllerPair)
            {
                var viewObject = EditorUtility.InstanceIDToObject(settings.pendingViewGameObjectID) as GameObject;
                var view = viewObject.AddComponent(Helper.GetType(settings.pendingViewClassName)) as MonoBehaviour;
                AddView(view);
                AddController(view);
                settings.isPendingViewControllerPair = false;
            }
        }

        static Field CreateField(ComponentCell cell)
        {
            var field = new Field();
            field.type = GetTypeName(cell.component.GetType());
            field.name = "m_" + cell.fieldName;
            field.AddAttributes("SerializeField");
            return field;
        }

        static Property CreateProperty(ComponentCell cell)
        {
            var prop = new Property();
            prop.protectionLevel = "public";
            prop.SetReadonly(true);
            prop.type = GetTypeName(cell.component.GetType());
            prop.name = cell.fieldName;
            prop.SetFieldName("m_" + cell.fieldName);
            return prop;
        }

        static string GetTypeName(System.Type t)
        {
            var n = t.ToString().Split('.');
            return n[n.Length - 1];
        }

        static void SetUpProject()
        {

        }

        static Class GetViewsDelegateClass()
        {
            var settings = MVCProjectSettings.Instance;
            var relativePath = settings.projectFolder + "/Views/ViewsDelegate.cs";
            var absolutePath = Helper.ConvertToAbsolutePath(relativePath);
            Class cls = null;
            if (File.Exists(absolutePath) == false)
            {
                cls = new Class("ViewsDelegate", "public");
                cls.AddInherited("DUICanvas");
                cls.nameSpace = settings.nameSpace + ".View";
                cls.AddAttribute("System.Serializable");
                cls.AddDirective("UnityEngine", "DynamicUI");
                cls.AddMember(new Method("void", "Init", "override", "public", "").AddLine("FindViews();").AddLine("base.Init();"));
                cls.AddMember(new Method("void", "FindViews", "", "", ""));
                cls.AddMember(new Method("void", "HideAll", "", "public", ""));
            }
            else
            {
                cls = new ClassParser().Parse(File.ReadAllText(absolutePath));
            }
            return cls;
        }

        static Class GetControllersDelegateClass()
        {
            var settings = MVCProjectSettings.Instance;
            var relativePath = settings.projectFolder + "/Controllers/ControllersDelegate.cs";
            var absolutePath = Helper.ConvertToAbsolutePath(relativePath);
            Class cls = null;
            if (File.Exists(absolutePath) == false)
            {
                cls = new Class("ControllersDelegate", "public");
                cls.nameSpace = settings.nameSpace + ".Controller";
                cls.AddMember(new Method("void", "Init", "", "public", ""));
            }
            else
            {
                cls = new ClassParser().Parse(File.ReadAllText(absolutePath));
            }
            return cls;
        }

        static void AddController(MonoBehaviour mono)
        {
            var script = MonoScript.FromMonoBehaviour(mono);
            var controllerScriptName = script.name.Remove(script.name.Length - 4, 4) + "Controller";
            Class controllerDelegateClass = GetControllersDelegateClass();
            if (controllerDelegateClass.members.Find(m => m.type == controllerScriptName) != null)
            {
                Debug.LogWarning(string.Format("Controller {0} already added!", script.name));
                return;
            }
            var fieldName = controllerScriptName.ToLowerFirst();
            controllerDelegateClass.AddMember(new Field(controllerScriptName, "m_" + fieldName));
            ((Method) controllerDelegateClass.members.Find(m => m.name == "Init"))
                .AddLine(string.Format("m_{0} = new {1}(App.Views.{2});",
                controllerScriptName.ToLowerFirst(),
                controllerScriptName,
                script.name.ToLowerFirst()));
            controllerDelegateClass.AddMember(new Property(controllerScriptName, fieldName, "public", "m_" + fieldName, "").SetReadonly(true));
            WriteControllersDelegateClass(controllerDelegateClass); 
        }

        static void WriteControllersDelegateClass(Class cls)
        {
            var settings = MVCProjectSettings.Instance;
            var absolutePath = Helper.ConvertToAbsolutePath(settings.projectFolder + "/Controllers/ControllersDelegate.cs");
            var str = @"///
///     GENERATED IN CODE
///     DO NOT MODIFY!
///
" + cls.ToString();
            File.WriteAllText(absolutePath, str);
        }

        static void WriteViewsDelegateClass(Class cls)
        {
            var settings = MVCProjectSettings.Instance;
            var absolutePath = Helper.ConvertToAbsolutePath(settings.projectFolder + "/Views/ViewsDelegate.cs");
            var str = @"///
///     GENERATED IN CODE
///     DO NOT MODIFY!
///
" + cls.ToString();
            File.WriteAllText(absolutePath, str);
        }

        public static void AddView(MonoBehaviour view)
        {
            var settings = MVCProjectSettings.Instance;

            var viewScript = MonoScript.FromMonoBehaviour(view);

            Class viewsDelegateClass = GetViewsDelegateClass();

            if (viewsDelegateClass.members.Find(m => m.type == viewScript.name) != null)
            {
                Debug.LogWarning(string.Format("View {0} already added!", viewScript.name));
                return;
            }

            var type = viewScript.name;
            var lowerCaseName = type[0].ToString().ToLower() + type.Substring(1);
            viewsDelegateClass.InsertMember(new Field(type, "m_" + lowerCaseName));
            viewsDelegateClass.InsertMember(new Property(type, lowerCaseName, "public", "m_" + lowerCaseName, "").SetReadonly(true));
            var initMethod = viewsDelegateClass.members.Find(m => m.name == "Init") as Method;
            var hideMethod = viewsDelegateClass.members.Find(m => m.name == "HideAll") as Method;
            var findViewsMethod = viewsDelegateClass.members.Find(m => m.name == "FindViews") as Method;
            findViewsMethod.AddLine("m_" + lowerCaseName + string.Format(@" = transform.FindChild(""{0}"").GetComponent<{1}>();", view.transform.GetPath(view.transform.parent), type));
            initMethod.AddLine("m_" + lowerCaseName + ".Init(this);");
            hideMethod.AddLine("m_" + lowerCaseName + ".HideImmediately();");

            WriteViewsDelegateClass(viewsDelegateClass);

            AssetDatabase.Refresh();
        }


        
        public static void CreateViewControllerPair(string name, GameObject target)
        {
            var nameSpace = MVCProjectSettings.Instance.nameSpace;
            var projectFolder = Helper.ConvertToAbsolutePath(MVCProjectSettings.Instance.projectFolder);
            var settings = MVCProjectSettings.Instance;
            
            #region Check Input

            if (string.IsNullOrEmpty(nameSpace))
            {
                Debug.LogWarning(string.Format("{0}", @"Creating view-controller pair failed. 
Provide a namespace in MVCProjectSettings object."));
                return;
            }
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning(string.Format("{0}", @"Creating view-controller pair failed. 
Provided name is empty."));
                return;
            }
            if (string.IsNullOrEmpty(projectFolder) || !Directory.Exists(projectFolder))
            {
                Debug.LogWarning(string.Format("{0}", @"Creating view-controller pair failed. 
Provide an existing project folder path in MVCProjectSettings object. 
Example: 'Assets/ProjectName'."));
                return;
            }

            #endregion Check Input

            var controllerFolder = projectFolder + "/Controllers";
            var viewFolder = projectFolder + "/Views";

            if (!Directory.Exists(controllerFolder))
                Directory.CreateDirectory(controllerFolder);
            if (!Directory.Exists(viewFolder))
                Directory.CreateDirectory(viewFolder);

            var baseControllerScriptPath = projectFolder + "/Controllers/Controller.cs";

            if(File.Exists(baseControllerScriptPath) == false)
            {
                var baseControllerScript = string.Format(@"using DynamicUI;

namespace {0}.Controller
{{
    public abstract class Controller<T> where T : DUIScreen
    {{
        public T view {{ get; protected set; }}
        public App app { get { return App.Instance; } }

        public Controller(T view)
        {{
            this.view = view;
            AddEventListeners();
        }}

        public virtual void AddEventListeners()
        {{
            view.onShow += OnShow;
        }}

        public virtual void OnShow() {{ }}
    }}
}}", nameSpace);
                File.WriteAllText(baseControllerScriptPath, baseControllerScript);
            }


            var controllerScript = string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DynamicUI;
using UnityEngine.UI;
using {1}.Model;
using {1}.View;

namespace {1}.Controller
{{
    public class {0}Controller : Controller<{0}View>
    {{
        public {0}Controller({0}View view) : base(view)
        {{

        }}

        public override void AddEventListeners()
        {{
            base.AddEventListeners();
        }}

        public override void OnShow()
        {{

        }}
    }}
}}", name, nameSpace);




            var viewScript = string.Format(@"using UnityEngine;
using DynamicUI;
using UnityEngine.UI;

namespace {0}.View
{{
    public class {1}View : DUIScreen
    {{
        public override void Init(DUICanvas canvas)
        {{
            base.Init(canvas);
        }}
    }}
}}
", nameSpace, name);

            var viewScriptPath = viewFolder + "/" + name + "View.cs";
            var controllerScriptPath = controllerFolder + "/" + name + "Controller.cs";

            if (File.Exists(viewScriptPath) || File.Exists(controllerScriptPath))
            {
                if (!EditorUtility.DisplayDialog("Warning", "View and/or Controller scripts files already exists. Overwrite?", "Yes", "Cancel"))
                    return;
            }
            settings.pendingViewClassName = nameSpace + "." + "View." + name + "View";
            settings.isPendingViewControllerPair = true;
            settings.pendingViewGameObjectID = target.GetInstanceID();
            EditorUtility.SetDirty(settings);

            File.WriteAllText(viewScriptPath, viewScript);
            File.WriteAllText(controllerScriptPath, controllerScript);
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(viewScriptPath));
            AssetDatabase.Refresh();
        }
    }

}
