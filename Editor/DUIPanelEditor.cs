using UnityEngine;
using System.Collections;
using UnityEditor;
using CodeGenerator;
namespace DynamicUI
{
    [CustomEditor(typeof(DUIPanel), true)]
    public class DUIPanelEditor : Editor
    {
        public DUIPanel panel { get { return (DUIPanel) target; } }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if(GUILayout.Button("Generate Code"))
            {

                var parser = new ClassParser();
                parser.Test();
            }
            if (GUILayout.Button("Bind Elements"))
            {

            }
        }

        void GenerateCode()
        {
            string[] directives = new string[] { "UnityEngine", "DynamicUI", "UnityEngine.UI" };
            string[] regions = new string[] { "Elements" };
            string[] inherited = new string[] { "DUIPanel"};
            var file = Application.dataPath + "/Scripts/" + panel.name + ".cs";

            var script = System.IO.File.OpenText(file);
            
            var parser = new ClassParser();
            parser.Test();
            var cls = parser.Parse(script.ReadToEnd());
            script.Close();
            //   var cls = new Class(Member.ProtectionLevel.Public, Class.ClassType.Sealed, "Bae", inherited, directives, regions);
            var script2 = System.IO.File.CreateText(Application.dataPath + "/Scripts/" + panel.name + ".cs");
              script2.Write(cls.ToString(0));
            script2.Close();
         
        }
    }
}
