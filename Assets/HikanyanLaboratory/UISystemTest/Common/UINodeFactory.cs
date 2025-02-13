using HikanyanLaboratory.UI;
using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
{
    public static class UINodeFactory
    {
        public static T Create<T>(string id, string prefabKey, IUINode parent = null) where T : UINodeBase
        {
            GameObject prefab = PrefabLoader.GetPrefab(prefabKey);
            if (prefab == null)
            {
                Debug.LogError($"Prefab for {typeof(T).Name} is null!");
                return null;
            }

            GameObject instance = Object.Instantiate(prefab);
            var node = instance.GetComponent<T>();

            if (node == null)
            {
                Debug.LogError($"Prefab {prefab.name} does not have a {typeof(T).Name} component attached.");
                return null;
            }

            node.Initialize(id, parent);
            return node;
        }
    }
}