using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.MVPStateTool
{
    [CreateAssetMenu(fileName = "MVPStateGeneratorSettings", menuName = "HikanyanLaboratory/MVPStateGeneratorSettings", order = 1)]
    public class MVPStateGeneratorSettings : ScriptableObject
    {
        public string OutputDirectory = "Assets/HikanyanLaboratory/UIToolSystem/MVPStateGenerator/Generated";
        public string NameSpace = "HikanyanLaboratory.UISystem";

        public List<string> WindowStates = new List<string> { "MainWindow", "SettingsWindow" };
        public List<string> ScreenStates = new List<string> { "MainScreen", "SettingsScreen" };

        public List<WindowData> WindowGenerators = new List<WindowData>();
        public List<ScreenData> ScreenGenerators = new List<ScreenData>();
    }

    [System.Serializable]
    public class WindowData
    {
        public string ScriptName;
        public GameObject Prefab;
    }
    
    [System.Serializable]
    public class WindowNodeInfo
    {
        public bool Generate;
        public string EnumName;
        public string ScriptName;
        public GameObject Prefab;
    }

    [System.Serializable]
    public class ScreenData
    {
        public string ScriptName;
        public GameObject Prefab;
    }
    
    [System.Serializable]
    public class ScreenNodeInfo
    {
        public bool Generate;
        public string EnumName;
        public string ScriptName;
        public GameObject Prefab;
    }
}