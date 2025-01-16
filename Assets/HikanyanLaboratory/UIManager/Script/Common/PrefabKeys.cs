using System.Collections.Generic;
public static class PrefabKeys
{
    private static Dictionary<string, string> _prefabPathDictionary = new Dictionary<string, string>()
    {
        { Cube, "Assets/HikanyanLaboratory/UIManager/Resources/Cube.prefab" },
    };

    public const string Cube = "Cube";
}