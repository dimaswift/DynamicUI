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
        Class mainClass, elementsSubClass;

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

        void GenerateCode()
        {

            string[] regions = new string[] { "Elements" };
            string[] inherited = new string[] { "DUIPanel"};
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            bool scriptExists = System.IO.File.Exists(scriptFilePath);
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
         
            elementsSubClass = new Class("Elements", "public", "sealed");
            List<Component> tmpList = new List<Component>();
            List<Component> elements = new List<Component>();
            foreach (var child in panel.GetComponentsInChildren<Transform>(true))
            {
                if (child == panel.transform) continue;
                tmpList.Clear();
                tmpList.AddRange(child.GetComponents<Component>());
                if(tmpList.Count > 1)
                {
                    tmpList.RemoveAll(ar => ar.GetType() == typeof(CanvasRenderer));
                    if(tmpList.Count == 1)
                        elements.Add(tmpList[0]);
                    else
                    {
                        tmpList.RemoveAll(ar => ar.GetType() == typeof(RectTransform));
                        elements.AddRange(tmpList);
                    }
                }
                else
                {
                    elements.Add(tmpList[0]);
                }
            }

            elementsSubClass.parentRegion = "Elements";
            mainClass.AddMember(elementsSubClass);
            mainClass.AddInherited("DUIPanel");
            mainClass.AddDirective("UnityEngine", "DynamicUI", "UnityEngine.UI");
            mainClass.AddRegion("Elements");
         
            ElementsConfirmationWindow.Open(SubmitComponents, elements);
        }

        public void SubmitComponents(List<Component> components)
        {
            foreach (var c in components)
            {
                var member = CreateMember(c);
                mainClass.AddMember(member);
            }
            string scriptFilePath = Application.dataPath + "/Scripts/" + panel.name + ".cs";
            File.WriteAllText(scriptFilePath, mainClass.ToString());
        }

        Member CreateMember(Component component)
        {
            Member member = null;
            return member;
        }
    }

    public class ElementsConfirmationWindow : EditorWindow
    {
        Vector2 scrollPos;
        System.Action<List<Component>> onSubmit;
        List<ComponentCell> cells;
        float cellSize = 30;
        GUIStyle typeLabel;

        public static void Open(System.Action<List<Component>> onSubmit, List<Component> components)
        {
            var w = GetWindow<ElementsConfirmationWindow>(true, "Select Elements you want", true);
            w.Show(true);
            w.minSize = new Vector2(400, 600);
            w.maxSize = new Vector2(400, 600);
            HandyEditor.CenterOnMainWin(w);
            w.onSubmit = onSubmit;
            w.cells = new List<ComponentCell>(components.Count);
            foreach (var c in components)
            {
                w.cells.Add(new ComponentCell(c));
            }
        }
      

        void OnEnable()
        {
            typeLabel = new GUIStyle() { alignment = TextAnchor.MiddleRight };
        }

        void OnGUI ()
        {
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
        }
      
        void Submit()
        {
            List<Component> components = new List<Component>(cells.Count);
            foreach (var c in cells)
            {
                if (c.selected)
                    components.Add(c.component);
            }
            onSubmit(components);
        }

        bool DrawComponent(ComponentCell c, ref Rect rect)
        {
            rect.y += 15;
            var color = GUI.color;
            GUI.color = c.selected ? Color.green.SetAlpha(.3f) : Color.red.SetAlpha(.3f);
            GUI.Box(new Rect(rect.x, rect.y, rect.width, cellSize - 3), "");
            GUI.color = color;
            GUI.Label(new Rect(rect.x, rect.y, 300, 16), c.component.name, EditorStyles.boldLabel);
            GUI.Label(new Rect(rect.width - 35, rect.y, 5, 16), c.type, typeLabel);
            c.selected = GUI.Toggle(new Rect(rect.width - 20, rect.y, 20, 20), c.selected, "");
            rect.y += cellSize - 15;
            return c.selected;
        }

        class ComponentCell
        {
            public Component component;
            public bool selected;
            public string type;
            public ComponentCell (Component c)
            {
                component = c;
                var t = c.GetType().ToString().Split('.');
                type = t[t.Length - 1];
                selected = true;
            }
        }

    }

}
