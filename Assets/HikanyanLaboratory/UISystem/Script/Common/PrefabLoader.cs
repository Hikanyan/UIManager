using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public static class PrefabLoader
{
    private static Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    private static Dictionary<string, string> prefabPathDictionary = new Dictionary<string, string>();

    public static void Initialize(string searchPath = "Assets")
    {
        prefabDictionary.Clear();
        prefabPathDictionary.Clear();

        List<string> resourcePaths = new List<string>();
        FindResourceDirectories(searchPath, resourcePaths);

        foreach (string path in resourcePaths)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { path });

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                string variableName = GenerateVariableName(prefab.name);

                if (!prefabDictionary.ContainsKey(variableName))
                {
                    prefabDictionary.Add(variableName, prefab);
                    prefabPathDictionary.Add(variableName, prefabPath);
                }
                else
                {
                    Debug.LogWarning($"Duplicate prefab name detected: {variableName}");
                }
            }
        }
        GeneratePrefabKeysClass();
    }

    private static void FindResourceDirectories(string path, List<string> resourcePaths)
    {
        foreach (string directory in Directory.GetDirectories(path))
        {
            if (Path.GetFileName(directory) == "Resources")
            {
                resourcePaths.Add(directory);
            }
            FindResourceDirectories(directory, resourcePaths);
        }
    }

    private static void GeneratePrefabKeysClass()
    {
        string classContent = "using System.Collections.Generic;\n";
        classContent += "public static class PrefabKeys\n{\n";
        classContent += "    private static Dictionary<string, string> _prefabPathDictionary = new Dictionary<string, string>()\n    {\n";

        foreach (var entry in prefabPathDictionary)
        {
            classContent += $"        {{ {entry.Key}, \"{entry.Value}\" }},\n";
        }
        classContent += "    };\n\n";

        foreach (var entry in prefabDictionary)
        {
            classContent += $"    public const string {entry.Key} = \"{entry.Key}\";\n";
        }
        classContent += "}";

        File.WriteAllText("Assets/HikanyanLaboratory/UIManager/Script/Common/PrefabKeys.cs", classContent);
        AssetDatabase.Refresh();
    }

    public static GameObject GetPrefab(string key)
    {
        if (prefabDictionary.TryGetValue(key, out GameObject prefab))
        {
            return prefab;
        }
        Debug.LogError($"Prefab with key '{key}' not found.");
        return null;
    }

    private static string GenerateVariableName(string prefabName)
    {
        return prefabName.Replace(" ", "_").Replace("-", "_");
    }
}
}