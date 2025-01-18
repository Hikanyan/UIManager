using System.Collections.Generic;
public static class PrefabKeys
{
    private static Dictionary<string, string> _prefabPathDictionary = new Dictionary<string, string>()
    {
        { Cube, "Assets/HikanyanLaboratory/UISystem/Resources/Cube.prefab" },
    };

    public const string Cube = "Cube";
public static IEnumerable<string> GetAllKeys()
        {
            return _prefabPathDictionary.Keys;
        }}