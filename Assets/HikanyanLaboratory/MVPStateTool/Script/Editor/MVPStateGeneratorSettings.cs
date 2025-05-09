using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.MVPStateTool
{
    [CreateAssetMenu(fileName = "MVPStateGeneratorSettings", menuName = "HikanyanLaboratory/MVPStateGeneratorSettings",
        order = 1)]
    public class MVPStateGeneratorSettings : ScriptableObject
    {
        public string OutputDirectory = "Assets/HikanyanLaboratory/UIToolSystem/MVPStateGenerator/Generated";
        public string TemplatesFolderPath = "Assets/HikanyanLaboratory/UIToolSystem/MVPStateGenerator/Templates";
        public string NameSpace = "HikanyanLaboratory.UISystem";

        public GameObject WindowTemplatePrefab;
        public GameObject ScreenTemplatePrefab;

        public List<string> WindowStates = new List<string> { "MainWindow", "SettingsWindow" };
        public List<string> ScreenStates = new List<string> { "MainScreen", "SettingsScreen" };

        public List<WindowData> WindowGenerators = new List<WindowData>();
        
        public List<ScreenDataGroup> ScreenGeneratorsByWindow = new List<ScreenDataGroup>();
    }

    public interface IDataEntry
    {
        string ScriptName { get; set; }
        GameObject Prefab { get; set; }
    }

    public interface IGeneratorNode
    {
        bool GenerateEnum { get; set; }
        bool GenerateScript { get; set; }
        string EnumName { get; set; }
        string ScriptName { get; set; }
        GameObject Prefab { get; set; }
    }

    [System.Serializable]
    public class WindowData : IDataEntry
    {
        [SerializeField] private string scriptName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private bool isGenerated;

        public string ScriptName
        {
            get => scriptName;
            set => scriptName = value;
        }

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }
        public bool IsGenerated
        {
            get => isGenerated;
            set => isGenerated = value;
        }
    }

    [System.Serializable]
    public class ScreenData : IDataEntry
    {
        [SerializeField] private string scriptName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private bool isGenerated;

        public string ScriptName
        {
            get => scriptName;
            set => scriptName = value;
        }

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }
        
        public bool IsGenerated
        {
            get => isGenerated;
            set => isGenerated = value;
        }
    }
    [System.Serializable]
    public class ScreenDataGroup
    {
        public string ParentWindowName;
        public List<ScreenData> Screens = new List<ScreenData>();
    }

    [System.Serializable]
    public class WindowNodeInfo : IGeneratorNode
    {
        [SerializeField] private bool generateEnum = true;
        [SerializeField] private bool generateScript = true;
        [SerializeField] private string enumName;
        [SerializeField] private string scriptName;
        [SerializeField] private GameObject prefab;

        public bool GenerateEnum
        {
            get => generateEnum;
            set => generateEnum = value;
        }

        public bool GenerateScript
        {
            get => generateScript;
            set => generateScript = value;
        }

        public string EnumName
        {
            get => enumName;
            set => enumName = value;
        }

        public string ScriptName
        {
            get => scriptName;
            set => scriptName = value;
        }

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }
    }

    [System.Serializable]
    public class ScreenNodeInfo : IGeneratorNode
    {
        [SerializeField] private bool generateEnum = true;
        [SerializeField] private bool generateScript = true;
        [SerializeField] private string enumName;
        [SerializeField] private string scriptName;
        [SerializeField] private GameObject prefab;
        [SerializeField] private bool isGenerated;
        public bool GenerateEnum
        {
            get => generateEnum;
            set => generateEnum = value;
        }

        public bool GenerateScript
        {
            get => generateScript;
            set => generateScript = value;
        }

        public string EnumName
        {
            get => enumName;
            set => enumName = value;
        }

        public string ScriptName
        {
            get => scriptName;
            set => scriptName = value;
        }

        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }
        public bool IsGenerated
        {
            get => isGenerated;
            set => isGenerated = value;
        }
    }
}