using System.Collections.Generic;
public static class PrefabKeys
{
    private static readonly Dictionary<string, string> PrefabPathDictionary = new Dictionary<string, string>()
    {
        { Cube, "Assets/HikanyanLaboratory/UIManager/Resources/Cube.prefab" },
        { SampleList, "Assets/HikanyanLaboratory/UISystem/Resources/SampleList.prefab" },
        { MainScene, "Assets/HikanyanLaboratory/UISystemTest/Resources/MainScene.prefab" },
        { MainWindow, "Assets/HikanyanLaboratory/UISystemTest/Resources/MainWindow.prefab" },
        { Screen1, "Assets/HikanyanLaboratory/UISystemTest/Resources/Screen1.prefab" },
        { Screen2, "Assets/HikanyanLaboratory/UISystemTest/Resources/Screen2.prefab" },
    };

    public const string Cube = "Cube";
    public const string SampleList = "SampleList";
    public const string MainScene = "MainScene";
    public const string MainWindow = "MainWindow";
    public const string Screen1 = "Screen1";
    public const string Screen2 = "Screen2";
    public static IEnumerable<string> GetAllKeys()
    {
        return PrefabPathDictionary.Keys;
    }
    public static string GetPrefabPath(string key)
    {
        return PrefabPathDictionary.TryGetValue(key, out var path) ? path : null;
    }}