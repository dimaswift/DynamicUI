using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HandyUtilities;
using DynamicUI;

public class MVCProjectSettings : SerializedSingleton<MVCProjectSettings>
{
    public string nameSpace = "MyProject";
    public string projectFolder = "Assets/Scripts";

    [HideInInspector]
    public string pendingViewClassName;
    [HideInInspector]
    public bool pendingViewCreation;
    [HideInInspector]
    public int pendingViewGameObjectID;
}
