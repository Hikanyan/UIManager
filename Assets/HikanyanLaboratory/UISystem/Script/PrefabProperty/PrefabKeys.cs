using System.Collections.Generic;
public static class PrefabKeys
{
    private static Dictionary<string, string> _prefabPathDictionary = new Dictionary<string, string>()
    {
        { SampleList, "Assets/HikanyanLaboratory/UISystem/Resources/SampleList.prefab" },
    };

    public const string SampleList = "SampleList";
   public static IEnumerable<string> GetAllKeys()
    {
        return _prefabPathDictionary.Keys;
    }
}