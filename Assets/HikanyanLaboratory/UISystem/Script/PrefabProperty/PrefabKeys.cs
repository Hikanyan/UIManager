using System.Collections.Generic;
public static class PrefabKeys
{
    private static readonly Dictionary<string, string> PrefabPathDictionary = new Dictionary<string, string>()
    {
        { Cube, "Assets/HikanyanLaboratory/UIManager/Resources/Cube.prefab" },
        { SampleList, "Assets/HikanyanLaboratory/UISystem/Resources/SampleList.prefab" },
    };

    public const string Cube = "Cube";
    public const string SampleList = "SampleList";
    public static IEnumerable<string> GetAllKeys()
    {
        return PrefabPathDictionary.Keys;
    }
    public static string GetPrefabPath(string key)
    {
        return PrefabPathDictionary.TryGetValue(key, out var path) ? path : null;
    }
}