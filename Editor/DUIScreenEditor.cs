using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using CodeGenerator;
using HandyUtilities;

namespace DynamicUI
{

    [CustomEditor(typeof(DUIScreen), true)]
    public class DUIScreenEditor : Editor
    {
        static string uiScreensScriptsPath { get { return scriptsFolder + "UIScreens.cs"; } }
        static string scriptsFolder { get { return Application.dataPath + "/" + DUISettings.Instance.UIRootFolder + "/"; } }
        public DUIScreen screen { get { return (DUIScreen) target; } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (screen.allowUpdatingElements && GUILayout.Button("Update Elements Bindings"))
            {
                UpdateElementsBindings(screen);
            }
            if (screen.allowUpdatingElements && GUILayout.Button("Delete Script and References"))
            {
                if(EditorUtility.DisplayDialog("Warning", "Are you sure you want to delete references and script file? It can cause compile time errors!", "Yes, delete", "Cancel"))
                {
                    DeleteScreen(screen);
                }
            }
            if (GUILayout.Button("Bind Existing Elements"))
            {
                screen.SendMessage("BindElements", SendMessageOptions.DontRequireReceiver);
            }
        }

        void OnEnable()
        {
            if (!File.Exists(uiScreensScriptsPath))
                SaveUIScreenScript(CreateUIScreensClass().ToString());
        }


        [MenuItem("Dynamic UI/Create DUI Script")]
        static void CreateDynamicUIScript()
        {
            ConfirmationTool.OpenWithArguments("Enter Name", "Create", (args) =>
            {
                var name = args[0] as string;

                var folder = HandyEditor.FindFolderInProject("DynamicUI");
                var scriptName = Helper.ConvertToAbsolutePath(folder + "/" + name + ".cs");
                if (File.Exists(scriptName))
                {
                    if (!EditorUtility.DisplayDialog("Warning", "File already exists. Overwrite it?", "Yes", "Cancel"))
                        return;
                }
                File.WriteAllText(scriptName, string.Format(@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using UnityEngine.UI;

namespace DynamicUI
{{
     public class {0} : DUIElement
     {{
        public override void Init()
        {{
            base.Init();

        }}
    }}
}}
", name));
                AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(scriptName));
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<MonoScript>(Helper.ConvertLoRelativePath(scriptName));
            },
            new ConfirmationTool.Label("Class Name", "DUINewScript"));
        }

        [MenuItem("Dynamic UI/Create UI Manager")]
        public static void CreateUIManager()
        {
            ConfirmationTool.OpenWithArguments("Enter Name", "Create", (args) =>
            {
                string name = args[0] as string;
                var nameSpace = args[1] as string;
                var folder = args[2] as string;
                DUISettings.Instance.UIManagerClassName = name;
                DUISettings.Instance.Namespace = nameSpace;
                DUISettings.Instance.UIRootFolder = folder;
                DUISettings.Instance.configured = true;
                DUISettings.Instance.pendingAddCanvasScript = true;
                EditorUtility.SetDirty(DUISettings.Instance);

                SaveUIScreenScript(CreateUIScreensClass().ToString());
                var cls = new Class(name, "public", "");
                cls.nameSpace = nameSpace;
                cls.AddInherited("DUICanvas");

                cls.AddDirective("UnityEngine", "DynamicUI", "HandyUtilities");
                cls.AddMember(new Field(name, "m_instance", "", "static").AddAttributes("SerializeField"));
                cls.AddMember(new Field("UIScreens", "m_screens").AddAttributes("SerializeField"));
                cls.AddMember(new Property("UIScreens", "screens", "public", "m_instance.m_screens", "static").SetReadonly(true));
                cls.AddMember(new Method("void", "Start", "", "", "").AddLine("m_instance = this;").AddLine("Init();"));
                cls.AddMember(new Method("void", "Init", "override", "public", "").AddLine("base.Init();").AddLine("screens.Init(this);"));
                var ft = File.CreateText(scriptsFolder + name + ".cs");
                ft.Write(cls.ToString());
                ft.Close();
                var canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    canvas = new GameObject(name).AddComponent<Canvas>();
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                }
                AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(scriptsFolder + name + ".cs"));

            },
            new ConfirmationTool.Label("Class Name", DUISettings.Instance.UIManagerClassName),
            new ConfirmationTool.Label("Namespace", DUISettings.Instance.Namespace),
            new ConfirmationTool.Label("Root Script Folder", DUISettings.Instance.UIRootFolder));

        }
        static void UpdateElementsBindings(Component screen)
        {

            ComponentPickerContainer.Instance.operationType = ComponentPickerContainer.OperationType.Update;
            EditorUtility.SetDirty(ComponentPickerContainer.Instance);
            string scriptFilePath = screen.GetComponent<MonoBehaviour>().GetScriptPath();
            if (File.Exists(scriptFilePath))
            {
                var classString = File.ReadAllText(scriptFilePath);
                var hasNameSpace = string.IsNullOrEmpty(DUISettings.Instance.Namespace) == false;
                var elementClass = new ClassParser().Parse(GetRegionChunk(classString, "Elements"), hasNameSpace ? 2 : 1);
            
                bool scriptExists = File.Exists(scriptFilePath);
                if (scriptExists == false)
                {
                    EditorUtility.DisplayDialog("Warning!", "Script " + screen.name + ".cs" +
                        " does not exist! Rename your screen or script!", "Okay");
                    return;
                }

                ComponentPickerContainer.Instance.pendingClass = elementClass;
                ComponentPicker.Open(SubmitUpdatedComponents, FilterDuplicateElements(screen), screen.gameObject);

                return;
            }
            else
            {
                Debug.LogError(string.Format("Script at path not found: {0}", scriptFilePath));
            }
        }

        void DeleteScreen(DUIScreen screen)
        {
            System.IO.File.Delete(scriptsFolder + screen.name + ".cs");
            var screensScript = GetScreensClass();
            screensScript.members.RemoveAll(m => m.type == screen.name);
            for (int i = 0; i < screensScript.members.Count; i++)
            {
                var m = screensScript.members[i] as Method;
                if(m != null)
                {
                    m.lines.RemoveAll(l => l.Contains(screen.name.ToLowerFirst()));
                }
            }
            File.WriteAllText(uiScreensScriptsPath, screensScript.ToString());
            Undo.DestroyObjectImmediate(screen);
        }


        [MenuItem("CONTEXT/RectTransform/Create DUIScreen")]
        static void CreateScreenScript(MenuCommand item)
        {
            ComponentPickerContainer.Instance.operationType = ComponentPickerContainer.OperationType.Create;
            EditorUtility.SetDirty(ComponentPickerContainer.Instance);
            var g = item.context as RectTransform;
            GenerateCode(g.gameObject, CreateScreenClass(g.gameObject), FilterComponents(g.gameObject));
        }

        public static string GetTypeName(System.Type t)
        {
            var n = t.ToString().Split('.');
            return n[n.Length - 1];
        }

        [MenuItem("CONTEXT/DUIScreen/Add Screen To Manager")]
        public static void AddScreen(MenuCommand command)
        {
            var screen = command.context as DUIScreen;
            AddScreen(screen);
        }

        public static Class CreateUIScreensClass()
        {
            var cls = new Class("UIScreens", "public", "sealed");
            cls.nameSpace = DUISettings.Instance.Namespace;
            cls.AddAttribute("System.Serializable");
            cls.AddDirective("UnityEngine", "DynamicUI");
            cls.AddMember(new Method("void", "Init", "", "public", "", new Method.Parameter("DUICanvas", "canvas")));
            cls.AddMember(new Method("void", "HideAll", "", "public", ""));
            return cls;
        }

        Class GetScreensClass()
        {
            if (!File.Exists(uiScreensScriptsPath))
                return null;
            var parser = new ClassParser().Parse(File.ReadAllText(uiScreensScriptsPath));
            return parser;
        }

        public static void AddScreen<T>(T screen) where T : DUIScreen
        {
            if (!File.Exists(uiScreensScriptsPath))
                SaveUIScreenScript(CreateUIScreensClass().ToString());
            var parser = new ClassParser().Parse(File.ReadAllText(uiScreensScriptsPath));
            if (parser.members.Find(m => m.type == screen.name) != null)
            {
                Debug.LogWarning(string.Format("UI Screen {0} already added!", screen.name));
                return;
            }
            var type = screen.GetType().ToString().Split('.').LastItem();
            var lowerCaseName = type[0].ToString().ToLower() + type.Substring(1);
            parser.InsertMember(new Field(type, "m_" + lowerCaseName).AddAttributes("SerializeField"));
            parser.InsertMember(new Property(type, lowerCaseName, "public", "m_" + lowerCaseName, "").SetReadonly(true));
            var initMethod = parser.members.Find(m => m.name == "Init") as Method;
            var hideMethod = parser.members.Find(m => m.name == "HideAll") as Method;
            initMethod.AddLine("m_" + lowerCaseName + ".Init(canvas);");
            hideMethod.AddLine("m_" + lowerCaseName + ".HideImmediately();");

            SaveUIScreenScript(parser.ToString());
            var binding = new ComponentPickerContainer.ScreenBinding();

            binding.targetGameObjectID = screen.gameObject.GetInstanceID();
            binding.fieldName = "m_" + lowerCaseName;
            binding.screenName = type;
            ComponentPickerContainer.Instance.screenBindings.Add(binding);
            EditorUtility.SetDirty(ComponentPickerContainer.Instance);
        }

        public static void SaveUIScreenScript(string body)
        {
            if (!Directory.Exists(scriptsFolder))
            {
                Directory.CreateDirectory(scriptsFolder);
            }
            var ft = File.CreateText(uiScreensScriptsPath);
            
            var comment = @"///
///     AUTO GENERATED FILE
///     DO NOT MODIFY
///
";
            var str = comment + body;
            ft.Write(str);
            ft.Close();
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(uiScreensScriptsPath));
        }

        public static void GenerateCode(GameObject screen, Class elementsClass, List<Component> components)
        {
            if (!Directory.Exists(scriptsFolder))
                Directory.CreateDirectory(scriptsFolder);
            string scriptFilePath = scriptsFolder + screen.name + ".cs";
            bool scriptExists = File.Exists(scriptFilePath);
            if (scriptExists)
            {
                if (!EditorUtility.DisplayDialog("Warning!", "Script " + screen.name + ".cs" +
                    " already exists. Overwrite it?", "Yes", "Cancel"))
                    return;
            }
            else
            {
                var ft = File.CreateText(scriptFilePath);
                ft.Close();
            }


            ComponentPickerContainer.Instance.pendingClass = elementsClass;
            ComponentPicker.Open(SubmitComponents, components, screen);
        }


        static List<Component> GetElements(Component screen)
        {
            var list = new List<Component>();
            var so = new SerializedObject(screen).FindProperty("m_elements");
            so.Copy();
            while (so.Next(true))
            {
                if (so.propertyType == SerializedPropertyType.ObjectReference)
                    list.Add(so.objectReferenceValue as Component);
            }
            return list;
        }

        static List<Component> FilterDuplicateElements(Component screen)
        {
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            var existingElements = GetElements(screen);

            foreach (var child in screen.GetComponentsInChildren<Transform>(true))
            {
                if (child == screen.transform) continue;
                tmpList.Clear();
                var components = child.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c is CanvasRenderer == false)
                    {
                        if (existingElements.FindIndex(e => e && e.GetInstanceID() == c.GetInstanceID()) < 0)
                        {
                            tmpList.Add(c);
                        }
                    }
                }
                if (tmpList.Count > 1)
                {
                    tmpList.RemoveAll(ar => ar is CanvasRenderer);

                    if (tmpList.Count == 1)
                        elements.Add(tmpList[0]);
                    else
                    {
                        elements.AddRange(tmpList);
                    }
                }
                else if (tmpList.Count > 0)
                {
                    elements.Add(tmpList[0]);
                }
            }
            return elements;
        }

        static List<Component> FilterComponents(GameObject screen)
        {
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            foreach (var child in screen.GetComponentsInChildren<Transform>(true))
            {
                if (child == screen.transform) continue;
                tmpList.Clear();
                var components = child.GetComponents<Component>();
                tmpList.AddRange(components);

                if (tmpList.Count > 1)
                {
                    tmpList.RemoveAll(ar => ar.GetType() == typeof(CanvasRenderer));

                    if (tmpList.Count == 1)
                        elements.Add(tmpList[0]);
                    else
                    {
                        elements.AddRange(tmpList);
                    }
                }
                else
                {
                    elements.Add(tmpList[0]);
                }
            }
            return elements;
        }

        static string AppendNamespace(string className) 
        {
            var n = DUISettings.Instance.Namespace;
            return string.IsNullOrEmpty(n) ? className : n + "." + className;
        }

        static Class CreateScreenClass(GameObject screen)
        {
            var elementsClass = new Class("Elements", "public")
            .SetParentRegion("Elements")
            .AddAttribute("System.Serializable");

            var mainClass = new Class(screen.name, "public")
            .AddMember(new Field("Elements", "m_elements").AddAttributes("SerializeField"))
            .AddMember(new Property("Elements", "elements", "public", "m_elements", "", "").SetReadonly(true))
            .AddInherited("DUIScreen")
            .AddDirective("UnityEngine", "DynamicUI", "UnityEngine.UI")
            .AddRegion("Initilization")
            .AddRegion("Elements")
            .AddRegion("Events")
            .AddMember(new Method("void", "Init", "override", "public", "Initilization")
                .AddLine("base.Init(canvas);")
                .AddParameters(new Method.Parameter("DUICanvas", "canvas")))
                .AddMember(new Method("void", "BindElements", "virtual", "protected").AddLine("elements.Bind(this);"))
            .AddMember(elementsClass);
            
            mainClass.nameSpace = DUISettings.Instance.Namespace;
            return mainClass;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            return;
            if (!File.Exists(uiScreensScriptsPath))
                SaveUIScreenScript(CreateUIScreensClass().ToString());
            var container = ComponentPickerContainer.Instance;
            GameObject screen = EditorUtility.InstanceIDToObject(container.targetID) as GameObject;
            if (container.pendingScriptCompile)
            {
                container.pendingScriptCompile = false;


                if (container.operationType == ComponentPickerContainer.OperationType.Create)
                {
                    var newPannel = screen.AddComponent(Helper.GetType(AppendNamespace(container.newTypeName)));
                    BindElements(new SerializedObject(newPannel).FindProperty("m_elements"));
                    AddScreen(new MenuCommand(newPannel));
                }
                else if (container.operationType == ComponentPickerContainer.OperationType.Update)
                {
                    var newPannel = screen.GetComponent(Helper.GetType(AppendNamespace(container.newTypeName)));
                    BindElements(new SerializedObject(newPannel).FindProperty("m_elements"));
                }
            }
            foreach (var binding in container.screenBindings)
            {

                screen = EditorUtility.InstanceIDToObject(binding.targetGameObjectID) as GameObject;
                 
                var canvas = screen.GetComponentInParent(Helper.GetType(AppendNamespace(DUISettings.Instance.UIManagerClassName)));
                if (canvas)
                {
                    var screenContainer = new SerializedObject(canvas).FindProperty("m_screens");
                    var p = screenContainer.FindPropertyRelative(binding.fieldName);
                    if (p == null) return;
                    p.objectReferenceValue = screen.GetComponent(binding.screenName);
                    screenContainer.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    Debug.Log(string.Format("{0}", "Please add UIManager.cs script first!"));
                }

            }
            container.screenBindings.Clear();
            EditorUtility.SetDirty(container);
        }

        public static void AddEventListenersToClass(Class mainClass, ComponentCell c, Method initMethod)
        {
            if (c.type == "Button" || c.type == "DUIButton")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Pressed";
                initMethod.AddLine(string.Format("elements.{0}.onClick.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events"));
            }
            else if (c.type == "Toggle")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Changed";
                initMethod.AddLine(string.Format("elements.{0}.onValueChanged.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events", new Method.Parameter("bool", "selected")));
            }
            else if (c.type == "Slider")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Changed";
                initMethod.AddLine(string.Format("elements.{0}.onValueChanged.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events", new Method.Parameter("float", "value")));
            }
            else if (c.type == "DUIToggle")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Changed";
                initMethod.AddLine(string.Format("elements.{0}.onToggleChange.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events", new Method.Parameter("bool", "selected")));
            }
            else if (c.type == "DUIRaycaster")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Down";
                initMethod.AddLine(string.Format("elements.{0}.onPointerDown.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events"));
            }
            else if (c.type == "InputField")
            {
                var callbackName = "On" + c.fieldName.ToUpperFirst() + "Changed";
                initMethod.AddLine(string.Format("elements.{0}.onValueChanged.AddListener({1});", c.fieldName, callbackName));
                mainClass.AddMember(new Method("void", callbackName, "", "", "Events", new Method.Parameter("string", "value")));
            }
        }



        public static void BindElements(SerializedProperty obj)
        {
            var container = ComponentPickerContainer.Instance;
            var fields = container.cells;


            foreach (var f in fields)
            {
                var p = obj.FindPropertyRelative("m_" + f.fieldName);
                if (p != null)
                    p.objectReferenceValue = f.component;
            }
            obj.serializedObject.ApplyModifiedProperties();
        }

        static string ReplaceRegion(string classString, string region, string newRegion)
        {
            int start = 0;
            int end = 0;
            var newRegionLines = newRegion.Split('\n');
            var classStringLines = new List<string>(classString.Split('\n'));
            for (int i = 0; i < classStringLines.Count; i++)
            {
                if (classStringLines[i].Contains("#region " + region))
                {
                    start = i + 1;
                }
                else if (classStringLines[i].Contains("#endregion " + region))
                {
                    end = i;
                }
            }


            classStringLines.RemoveRange(start, end - start);


            for (int i = newRegionLines.Length - 1; i >= 0; i--)
            {
                classStringLines.Insert(start, newRegionLines[i]);
            }

            return string.Join("\n", classStringLines.ToArray());
        }


        static string GetRegionChunk(string classString, string region)
        {
            int start = 0;
            int end = 0;
            var classStringLines = classString.Split('\n');
            for (int i = 0; i < classStringLines.Length; i++)
            {
                if (classStringLines[i].Contains("#region " + region))
                {
                    start = i + 1;
                }
                else if (classStringLines[i].Contains("#endregion " + region))
                {
                    end = i;
                }
            }
            var regionString = "";
            for (int i = start; i < end; i++)
            {
                regionString += classStringLines[i] + '\n';
            }
            return regionString;
        }

        public static void SubmitUpdatedComponents(List<ComponentCell> components, GameObject screen)
        {
            var elementsClass = ComponentPickerContainer.Instance.pendingClass;

            var bindMethod = new Method("void", "Bind", "", "public", "", new Method.Parameter(screen.name, "screen"));
            bindMethod.AddLine("#if UNITY_EDITOR");
            bindMethod.AddLine("var root = screen.transform;");
            bindMethod.AddLine(@"var so = new UnityEditor.SerializedObject(screen).FindProperty(""m_elements"");");

            foreach (var c in components)
            {
                var f = CreateField(c);
                elementsClass.AddMember(f);
                bindMethod.AddLine(string.Format(@"so.FindPropertyRelative(""{0}"").objectReferenceValue = root.FindChild(""{1}"").GetComponent<{2}>();", f.name, c.component.transform.GetPath(screen.transform), f.type));
            }
            bindMethod.AddLine("so.serializedObject.ApplyModifiedProperties();");
            bindMethod.AddLine("UnityEditor.EditorUtility.SetDirty(screen);");
            bindMethod.AddLine("#endif");
            elementsClass.AddMember(bindMethod);
            foreach (var c in components)
            {
                var p = CreateProperty(c);
                elementsClass.AddMember(p);
            }

            string scriptFilePath = scriptsFolder + screen.name + ".cs";
            var hasNameSpace = string.IsNullOrEmpty(DUISettings.Instance.Namespace) == false;
            File.WriteAllText(scriptFilePath, ReplaceRegion(File.ReadAllText(scriptFilePath), "Elements", elementsClass.ToString(hasNameSpace ? 2 :1)));
            var container = ComponentPickerContainer.Instance;
            container.pendingScriptCompile = true;
            container.newTypeName = screen.name;
            container.targetID = screen.GetInstanceID();
            EditorUtility.SetDirty(ComponentPickerContainer.Instance);
            Undo.RecordObject(screen.gameObject, "Screen");

            EditorUtility.SetDirty(screen);
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(scriptFilePath));
        }

        public static void SubmitComponents(List<ComponentCell> components, GameObject screen)
        {
            var mainClass = ComponentPickerContainer.Instance.pendingClass;
            Class elementsClass = mainClass.members.Find(e => e.name == "Elements" && e is Class) as Class;
            var initMethod = mainClass.members.Find(m => m.name == "Init") as Method;
            var bindMethod = new Method("void", "Bind", "", "public", "", new Method.Parameter(mainClass.name, "screen"));
            bindMethod.AddLine("#if UNITY_EDITOR");
            bindMethod.AddLine("var root = screen.transform;");
            bindMethod.AddLine(@"var so = new UnityEditor.SerializedObject(screen).FindProperty(""m_elements"");");
            foreach (var c in components)
            {
                var f = CreateField(c);
                AddEventListenersToClass(mainClass, c, initMethod);
                elementsClass.AddMember(f);
                bindMethod.AddLine(string.Format(@"so.FindPropertyRelative(""{0}"").objectReferenceValue = root.FindChild(""{1}"").GetComponent<{2}>();", f.name, c.component.transform.GetPath(screen.transform), f.type));
            }
            bindMethod.AddLine("so.serializedObject.ApplyModifiedProperties();");
            bindMethod.AddLine("UnityEditor.EditorUtility.SetDirty(screen);");
            bindMethod.AddLine("#endif");
            elementsClass.AddMember(bindMethod);
            foreach (var c in components)
            {
                var p = CreateProperty(c);
                elementsClass.AddMember(p);
            }
            string scriptFilePath = scriptsFolder + screen.name + ".cs";
            File.WriteAllText(scriptFilePath, mainClass.ToString());
            var container = ComponentPickerContainer.Instance;
            container.pendingScriptCompile = true;
            container.newTypeName = mainClass.name;
            container.targetID = screen.gameObject.GetInstanceID();
            var hasNameSpace = string.IsNullOrEmpty(DUISettings.Instance.Namespace) == false;
            container.elementsClassString = elementsClass.ToString(hasNameSpace ? 2 : 1);

            EditorUtility.SetDirty(ComponentPickerContainer.Instance);
            Undo.RecordObject(screen.gameObject, "Screen");

            EditorUtility.SetDirty(screen);
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(scriptFilePath));
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
    }

  


}
