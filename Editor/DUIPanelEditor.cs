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
        public DUIPanel panel { get { return (DUIPanel) target; } }
        readonly string[] IGNORED_UI_ELEMENTS = new string[] { "CanvasRenderer", "Toggle" };
        Class mainClass, elementsClass;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Generate Code"))
            {
                GenerateCode();
            }
            if (GUILayout.Button("Bind Elements"))
            {

            }
        }

        public static string GetTypeName(System.Type t)
        {
            var n = t.ToString().Split('.');
            return n[n.Length - 1];
        }

        void GenerateCode()
        {

            string[] regions = new string[] { "Elements" };
            string[] inherited = new string[] { "DUIPanel"};
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            bool scriptExists = File.Exists(scriptFilePath);
            if (scriptExists)
            {
                if(!EditorUtility.DisplayDialog("Warning!", "Script " + panel.name + ".cs" + " already exists. Overwrite it?", "Yes", "Cancel"))
                    return;
            }
            else
            {
                File.WriteAllText(scriptFilePath, "");
            }
           

            mainClass = new Class(panel.name, "public");
         
            elementsClass = new Class("Elements", "public", "sealed");
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            var container = ComponentCellContainer.Instance;

            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child == panel.transform) continue;
                tmpList.Clear();
                var components = child.GetComponents<Component>();
                tmpList.AddRange(components);

                if(tmpList.Count > 1)
                {
                    tmpList.RemoveAll(ar => ar.GetType() == typeof(CanvasRenderer));
                    if(tmpList.Count == 1)
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

            elementsClass.parentRegion = "Elements";
            elementsClass.AddAttribute("System.Serializable");
            mainClass.AddMember(elementsClass);
            mainClass.AddMember(new Field("Elements", "m_elements").AddAttribute("SerializeField"));
            mainClass.AddMember(new Property("Elements", "elements", "public", "m_elements", "", "").SetReadonly(true));
            mainClass.AddInherited("DUIPanel");
            mainClass.AddDirective("UnityEngine", "DynamicUI", "UnityEngine.UI");
            mainClass.AddRegion("Initilization");
            mainClass.AddRegion("Elements");
            mainClass.AddMember(new Method("void", "Init", "override", "public", "Initilization").AddLine("base.Init()"));


            ElementsConfirmationWindow.Open(SubmitComponents, elements);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var container = ComponentCellContainer.Instance;
            if (container.pendingScriptCompile)
            {
                container.pendingScriptCompile = false;
                EditorUtility.SetDirty(container);
                DUIPanel panel = EditorUtility.InstanceIDToObject(container.panelID) as DUIPanel;
                var newPannel = panel.gameObject.AddComponent(Helper.GetType(container.newTypeName));

                BindElements(new SerializedObject(newPannel).FindProperty("m_elements"));
                DestroyImmediate(panel);
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

        public void SubmitComponents(List<ComponentCell> components)
        {
            foreach (var c in components)
            {
                var f = CreateField(c);
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
            container.panelID = panel.GetInstanceID();
            container.elementsClass = elementsClass.ToString();

            EditorUtility.SetDirty(ComponentCellContainer.Instance);
            Undo.RecordObject(panel.gameObject, "Panel");

            EditorUtility.SetDirty(target);  
            AssetDatabase.ImportAsset(Helper.ConvertLoRelativePath(scriptFilePath));
        }

        Field CreateField(ComponentCell cell)
        {
            var field = new Field();
            field.type = GetTypeName(cell.component.GetType());
            field.name = "m_" + cell.fieldName;
            field.AddAttribute("SerializeField");
            return field;
        }

        Property CreateProperty(ComponentCell cell)
        {
            var prop = new Property();
            prop.protectionLevel = "public";
            prop.readOnly = true;
            prop.type = GetTypeName(cell.component.GetType());
            prop.name = cell.fieldName;
            prop.fieldName = "m_" + cell.fieldName;
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
        public ComponentCell(Component c)
        {
        
            componentID = c.GetInstanceID();
            var t = c.GetType().ToString().Split('.');
            type = t[t.Length - 1];
            objectName = component.name;
            selected = true;
            fieldName = System.Text.RegularExpressions.Regex.Replace(component.name, @"[\s*\(\)-]", ""); ;
        }
    }

    public class ElementsConfirmationWindow : EditorWindow
    {
        Vector2 scrollPos;
        System.Action<List<ComponentCell>> onSubmit;
        float cellSize = 30;
        GUIStyle typeLabel;

        public static void Open(System.Action<List<ComponentCell>> onSubmit, List<Component> components)
        {
            var w = GetWindow<ElementsConfirmationWindow>(true, "Select Elements you want", true);
            w.Show(true);
            w.minSize = new Vector2(400, 600);
            w.maxSize = new Vector2(400, 600);
            HandyEditor.CenterOnMainWin(w);
            w.onSubmit = onSubmit;
            ComponentCellContainer.Instance.cells.Clear();
            foreach (var c in components)
            {
                ComponentCellContainer.Instance.cells.Add(new ComponentCell(c));
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
            typeLabel = new GUIStyle() { alignment = TextAnchor.MiddleRight };
        }

        void OnGUI ()
        {
            Undo.RecordObject(ComponentCellContainer.Instance, "Components Container");
            var cells = ComponentCellContainer.Instance.cells;
            var rect = new Rect(5, 5, position.width - 10, position.height - 50);
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

            if (GUI.Button(new Rect(rect.x, position.height - 25, rect.width, 20), "Submit"))
            {
                Submit();
            }

            if (GUI.Button(new Rect(rect.x, position.height - 55, rect.width, 20), "Remove Duplicates"))
            {
                RemoveDuplicates();
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
            onSubmit(ComponentCellContainer.Instance.cells);
            Close();
        }

        bool DrawComponent(ComponentCell c, ref Rect rect)
        {
            rect.y += 15;
            var color = GUI.color;
            GUI.color = c.selected ? Color.green.SetAlpha(.3f) : Color.red.SetAlpha(.3f);
            GUI.Box(new Rect(rect.x, rect.y, rect.width, cellSize - 3), "");
            GUI.color = color;
            c.fieldName = GUI.TextField(new Rect(rect.x, rect.y, 300, 16), c.fieldName, EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.width - 35, rect.y, 5, 16), c.type, typeLabel);
            c.selected = GUI.Toggle(new Rect(rect.width - 20, rect.y, 20, 20), c.selected, "");
            rect.y += cellSize - 15;
            return c.selected;
        }
    }

}
