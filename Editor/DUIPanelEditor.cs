using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using CodeGenerator;
using HandyUtilities;

namespace DynamicUI
{

    [CustomEditor(typeof(DUIPanel), true)]
    public class DUIPanelEditor : Editor
    {
        static string uiScreensScriptsPath { get { return scriptsFolder + "UIScreens.cs"; } }
        static string scriptsFolder { get { return Application.dataPath + "/Scripts/"; } }
        public DUIPanel panel { get { return (DUIPanel) target; } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Update Elements Bindings"))
            {
                UpdateElementsBindings(panel);
            }
        }

        void OnEnable()
        {
            if (!File.Exists(uiScreensScriptsPath))
                SaveUIScreenScript(CreateUIScreensClass().ToString());
        }
        void Test()
        {
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            var classString = File.ReadAllText(scriptFilePath);
            var mainClass = new ClassParser().Parse(classString);
            Class elementsClass = mainClass.members.Find(m => m is Class && m.name == "Elements") as Class;
            elementsClass.AddMember(new Field("string", "m_ass", "").AddAttributes("SerializeField"));
            elementsClass.AddMember(new Property("string", "ass", "public", "m_ass", "", "", "").SetReadonly(true));
            Debug.Log(string.Format("{0}", mainClass.ToString()));
        }

        static void UpdateElementsBindings(Component panel)
        {
            ComponentCellContainer.Instance.operationType = ComponentCellContainer.OperationType.Update;
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            if (File.Exists(scriptFilePath))
            {
                var classString = File.ReadAllText(scriptFilePath);
                var mainClass = new ClassParser().Parse(classString);
                GenerateCode(panel.gameObject, mainClass, FilterDuplicateElements(panel));
                return;
            }
            else
            {
                Debug.LogError(string.Format("Script at path not found: {0}", scriptFilePath));
            }
        }

        [MenuItem("CONTEXT/RectTransform/Create DUIPanel")]
        static void CreatePanelScript(MenuCommand item)
        {
            ComponentCellContainer.Instance.operationType = ComponentCellContainer.OperationType.Create;
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
            var g = item.context as RectTransform;
            GenerateCode(g.gameObject, CreatePanelClass(g.gameObject), FilterComponents(g.gameObject));
        }

        public static string GetTypeName(System.Type t)
        {
            var n = t.ToString().Split('.');
            return n[n.Length - 1];
        }

        [MenuItem("CONTEXT/DUIPanel/Add Screen To Manager")]
        public static void AddScreen(MenuCommand command)
        {
            var panel = command.context as DUIPanel;
            AddScreen(panel);
        }

        public static Class CreateUIScreensClass()
        {
            var cls = new Class("UIScreens", "public", "sealed");
            cls.AddAttribute("System.Serializable");
            cls.AddDirective("UnityEngine", "DynamicUI");
            cls.AddMember(new Method("void", "Init", "", "public", "", new Method.Parameter("DUICanvas", "canvas")));
            cls.AddMember(new Method("void", "HideAll", "", "public", ""));
            return cls;
        }

        public static void AddScreen<T>(T screen) where T : DUIPanel
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
            hideMethod.AddLine("m_" + lowerCaseName + ".Hide();");
            SaveUIScreenScript(parser.ToString());
            var binding = new ComponentCellContainer.ScreenBinding();

            binding.panelID = screen.gameObject.GetInstanceID();
            binding.fieldName = "m_" + lowerCaseName;
            binding.screenName = type;
            ComponentCellContainer.Instance.screenBindings.Add(binding);
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
        }

        public static void SaveUIScreenScript(string body)
        {
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

        public static void GenerateCode(GameObject panel, Class mainClass, List<Component> components)
        {
            if (!Directory.Exists(scriptsFolder))
                Directory.CreateDirectory(scriptsFolder);
            string scriptFilePath = scriptsFolder + panel.name + ".cs";
            bool scriptExists = File.Exists(scriptFilePath);
            if (scriptExists)
            {
                if (!EditorUtility.DisplayDialog("Warning!", "Script " + panel.name + ".cs" +
                    " already exists. Overwrite it?", "Yes", "Cancel"))
                    return;
            }
            else
            {
                var ft = File.CreateText(scriptFilePath);
                ft.Close();
            }


            ComponentCellContainer.Instance.mainClass = mainClass;
            ElementsConfirmationWindow.Open(SubmitComponents, components, panel);
        }

        static List<Component> GetElements(Component panel)
        {
            var list = new List<Component>();
            var so = new SerializedObject(panel).FindProperty("m_elements");
            so.Copy();
            while (so.Next(true))
            {
                if (so.propertyType == SerializedPropertyType.ObjectReference)
                    list.Add(so.objectReferenceValue as Component);
            }
            return list;
        }

        static List<Component> FilterDuplicateElements(Component panel)
        {
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            var existingElements = GetElements(panel);

            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child == panel.transform) continue;
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

        static List<Component> FilterComponents(GameObject panel)
        {
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child == panel.transform) continue;
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

        static Class CreatePanelClass(GameObject panel)
        {
            var elementsClass = new Class("Elements", "public")
            .SetParentRegion("Elements")
            .AddAttribute("System.Serializable");

            var mainClass = new Class(panel.name, "public")
            .AddMember(new Field("Elements", "m_elements").AddAttributes("SerializeField"))
            .AddMember(new Property("Elements", "elements", "public", "m_elements", "", "").SetReadonly(true))
            .AddInherited("DUIPanel")
            .AddDirective("UnityEngine", "DynamicUI", "UnityEngine.UI")
            .AddRegion("Initilization")
            .AddRegion("Elements")
            .AddRegion("Events")
            .AddMember(new Method("void", "Init", "override", "public", "Initilization")
                .AddLine("base.Init(canvas)")
                .AddParameters(new Method.Parameter("DUICanvas", "canvas")))
            .AddMember(elementsClass);

            return mainClass;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (!File.Exists(uiScreensScriptsPath))
                SaveUIScreenScript(CreateUIScreensClass().ToString());
            var container = ComponentCellContainer.Instance;
            GameObject panel = EditorUtility.InstanceIDToObject(container.panelID) as GameObject;
            if (container.pendingScriptCompile)
            {
                container.pendingScriptCompile = false;


                if (container.operationType == ComponentCellContainer.OperationType.Create)
                {
                    var newPannel = panel.AddComponent(Helper.GetType(container.newTypeName));
                    BindElements(new SerializedObject(newPannel).FindProperty("m_elements"));
                    AddScreen(new MenuCommand(newPannel));
                }
                else if (container.operationType == ComponentCellContainer.OperationType.Update)
                {
                    var newPannel = panel.GetComponent(Helper.GetType(container.newTypeName));
                    BindElements(new SerializedObject(newPannel).FindProperty("m_elements"));
                }
            }
            foreach (var binding in container.screenBindings)
            {
                panel = EditorUtility.InstanceIDToObject(binding.panelID) as GameObject;
                if (!panel)
                    continue;
                var canvas = panel.GetComponentInParent(System.Type.GetType("UIManager"));
                if(canvas)
                {
                    var screenContainer = new SerializedObject(canvas).FindProperty("m_screens");
                    screenContainer.FindPropertyRelative(binding.fieldName).objectReferenceValue = panel.GetComponent(binding.screenName);
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
            var container = ComponentCellContainer.Instance;
            var fields = container.cells;


            foreach (var f in fields)
            {
                var p = obj.FindPropertyRelative("m_" + f.fieldName);
                if(p != null)
                    p.objectReferenceValue = f.component;
            }
            obj.serializedObject.ApplyModifiedProperties();
        }

        public static void SubmitComponents(List<ComponentCell> components, GameObject panel)
        {
            var mainClass = ComponentCellContainer.Instance.mainClass;
            Class elementsClass = mainClass.members.Find(m => m is Class && m.name == "Elements") as Class;
            var initMethod = mainClass.members.Find(m => m.name == "Init") as Method;
            
            foreach (var c in components)
            {
                var f = CreateField(c);
                AddEventListenersToClass(mainClass, c, initMethod);
                elementsClass.AddMember(f);
            }
            foreach (var c in components)
            {
                var p = CreateProperty(c);
                elementsClass.AddMember(p);
            } 
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            File.WriteAllText(scriptFilePath, mainClass.ToString());
            var container = ComponentCellContainer.Instance;
            container.pendingScriptCompile = true;
            container.newTypeName = mainClass.name;
            container.panelID = panel.gameObject.GetInstanceID();
            container.elementsClassString = elementsClass.ToString();

            EditorUtility.SetDirty(ComponentCellContainer.Instance);
            Undo.RecordObject(panel.gameObject, "Panel");

            EditorUtility.SetDirty(panel);  
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

    [System.Serializable]
    public class ComponentCell
    {
        public Component component { get { return EditorUtility.InstanceIDToObject(componentID) as Component; } }
        public int componentID;
        public bool selected;
        public string objectName;
        public string fieldName;
        public string type;
        public int depth;
 
        public ComponentCell(Component c, GameObject panel)
        {
            
            componentID = c.GetInstanceID();
            var t = c.GetType().ToString().Split('.');
            type = t[t.Length - 1];
            objectName = component.name;
            selected = true;
            fieldName = System.Text.RegularExpressions.Regex.Replace(component.name, @"[\s*\(\)-]", "");
            fieldName = char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1);
            var parent = c.transform.parent;
            while(parent != null && parent != panel.transform)
            {
                depth++;
                parent = parent.parent;
            }
        }
    }

    public class ElementsConfirmationWindow : EditorWindow
    {
        Vector2 scrollPos;
        System.Action<List<ComponentCell>, GameObject> onSubmit;
        float cellSize = 40;
        GUIStyle typeLabel;
        GameObject panel;

        public static void Open(System.Action<List<ComponentCell>, GameObject> onSubmit, List<Component> components, GameObject panel)
        {
            var w = GetWindow<ElementsConfirmationWindow>(true, "Select Elements you want", true);
            w.panel = panel;
            w.Show(true);
            w.minSize = new Vector2(600, 600);
            w.maxSize = new Vector2(600, 600);
            HandyEditor.CenterOnMainWin(w);
            w.onSubmit = onSubmit;
            var container = ComponentCellContainer.Instance;
            container.cells.Clear();
            foreach (var c in components)
            {
                container.cells.Add(new ComponentCell(c, panel));
            }
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
            Undo.undoRedoPerformed += w.OnUndo;
        }
        
        void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndo;
        }

        void OnEnable()
        {
            typeLabel = new GUIStyle() { alignment = TextAnchor.MiddleRight, fontStyle = FontStyle.Bold };
        }

        void OnGUI ()
        {
            Undo.RecordObject(ComponentCellContainer.Instance, "Components Container");
            var cells = ComponentCellContainer.Instance.cells;
            float scrollViewSize = position.height - 120;
            var rect = new Rect(5, 5, position.width - 10, scrollViewSize);
            GUI.Box(rect, "Found " + cells.Count + " elements:", EditorStyles.boldLabel);
            var scrollView = new Rect(rect.x, 20, position.width - 25, (cellSize * cells.Count) + 45);
            scrollPos = GUI.BeginScrollView(new Rect(5, 20, rect.width, rect.height), scrollPos, scrollView, false, true);

            rect.y += 20;
            var elementRect = rect;
            elementRect.width = scrollView.width;
            for (int i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                DrawComponent(cell, ref elementRect);
            }
            GUI.EndScrollView();

          
            if (GUI.Button(new Rect(rect.x, position.height - 100, rect.width, 20), "Add Type To Names"))
            {
                AddTypeToNames();
            }
            if (GUI.Button(new Rect(rect.x, position.height - 75, rect.width, 20), "Remove Duplicates"))
            {
                RemoveDuplicates();
            }
            if (GUI.Button(new Rect(rect.x, position.height - 50, rect.width, 20), "Remove All"))
            {
                Undo.RecordObject(ComponentCellContainer.Instance, "Components Container");
                cells.ForEach(c => c.selected = false);
                EditorUtility.SetDirty(ComponentCellContainer.Instance);
            }
            if (GUI.Button(new Rect(rect.x, position.height - 25, rect.width, 20), "Submit"))
            {
                Submit();
            }
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
        }

        void AddTypeToNames()
        {
            Undo.RecordObject(ComponentCellContainer.Instance, "Components Container");
            var cells = ComponentCellContainer.Instance.cells;
            foreach (var cell in cells)
            {
                cell.fieldName +=  cell.type;
            }
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
        }

        void RemoveDuplicates()
        {
            Undo.RecordObject(ComponentCellContainer.Instance, "Components Container");
            var cells = ComponentCellContainer.Instance.cells;
            foreach (var  cell in cells)
            {
                foreach (var cell2 in cells)
                {
                    if(cell2.selected && cell.selected && cell.fieldName == cell2.fieldName &&
                        cell.component != cell2.component)
                    {
                        if (cell.type == "Image" && cell2.type == "Button")
                            cell.selected = false;
                        else if (cell2.type == "Image" && cell.type == "Button")
                            cell2.selected = false;
                        else if(cell.type == "RectTransform" && cell2.type == "Image")
                            cell2.selected = false;
                        else if (cell.type == "RectTransform" && cell2.type == "Text")
                            cell.selected = false;
                        else if (cell.type == "RectTransform" && cell2.type == "Button")
                            cell.selected = false;
                    }
                }
            }
            EditorUtility.SetDirty(ComponentCellContainer.Instance);
        }

        void OnUndo()
        {
            Repaint();
        }
      
        void Submit()
        {
            ComponentCellContainer.Instance.cells.RemoveAll(c => c.selected == false);
            onSubmit(ComponentCellContainer.Instance.cells, panel);
            Close();
        }

        bool DrawComponent(ComponentCell c, ref Rect rect)
        {
            rect.y += 15;
            var color = GUI.color;
            float depth = c.depth * 15;
            GUI.color = c.selected ? Color.green.SetAlpha(.3f) : Color.red.SetAlpha(.3f);
            GUI.enabled = c.selected;
            GUI.Box(new Rect(rect.x + depth, rect.y, rect.width, cellSize - 3), "");
            GUI.color = color;

            c.fieldName = GUI.TextField(new Rect(rect.x + depth + 5, rect.y + 8, 200, 22), c.fieldName, EditorStyles.largeLabel);
          //  GUI.Label(new Rect(rect.x + depth, rect.y + 18, 300, 16), c.component.name, EditorStyles.miniLabel);
            GUI.Label(new Rect(rect.width - 35, rect.y + 8, 5, 16), string.Format("{0} ({1})", c.component.name, c.type), typeLabel);
            GUI.enabled = true;
            c.selected = GUI.Toggle(new Rect(rect.width - 20, rect.y + 10, 20, 20), c.selected, "");
            rect.y += cellSize - 15;
            return c.selected;
        }
    }

}
