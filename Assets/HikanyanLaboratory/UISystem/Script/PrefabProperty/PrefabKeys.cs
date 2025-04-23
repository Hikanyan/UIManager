using System.Collections.Generic;
public static class PrefabKeys
{
    private static readonly Dictionary<string, string> PrefabPathDictionary = new Dictionary<string, string>()
    {
        { MainScene, "Assets/HikanyanLaboratory/UIManager/Resources/MainScene.prefab" },
        { MainWindow, "Assets/HikanyanLaboratory/UIManager/Resources/MainWindow.prefab" },
        { Screen1, "Assets/HikanyanLaboratory/UIManager/Resources/Screen1.prefab" },
        { Screen2, "Assets/HikanyanLaboratory/UIManager/Resources/Screen2.prefab" },
        { SampleList, "Assets/HikanyanLaboratory/UISystem/Resources/SampleList.prefab" },
    };

    public const string MainScene = "MainScene";
    public const string MainWindow = "MainWindow";
    public const string Screen1 = "Screen1";
    public const string Screen2 = "Screen2";
    public const string SampleList = "SampleList";
    public static IEnumerable<string> GetAllKeys()
    {
        return PrefabPathDictionary.Keys;
    }
    public static string GetPrefabPath(string key)
    {
        return PrefabPathDictionary.TryGetValue(key, out var path) ? path : null;
    }}