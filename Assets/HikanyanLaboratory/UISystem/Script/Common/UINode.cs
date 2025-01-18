using UnityEngine;
using System.Collections.Generic;

namespace HikanyanLaboratory.UI
{
    public abstract class UINode : MonoBehaviour
    {
        public class Id
        {
            private readonly string _name;
            public readonly string DefaultPrefabName;

            public Id(string name)
            {
                _name = name;
            }

            public Id(string name, string defaultPrefabName)
            {
                _name = name;
                DefaultPrefabName = defaultPrefabName;
            }

            public override int GetHashCode() => _name.GetHashCode();

            public override bool Equals(object obj)
            {
                Id other = obj as Id;
                return other != null && _name == other._name;
            }

            public override string ToString() => _name;
        }

        public class Data
        {
            private Dictionary<string, object> _data = new Dictionary<string, object>();

            public void Add(string key, object value) => _data[key] = value;

            public T Get<T>(string key)
            {
                if (_data.TryGetValue(key, out var value))
                    return (T)value;
                throw new KeyNotFoundException($"Key '{key}' not found in Data.");
            }
        }

        public Id ID { get; private set; }
        public string PrefabName { get; private set; }

        protected readonly List<UINode> ChildNodes = new List<UINode>();

        public virtual void OnInitialize()
        {
        }

        public virtual void OnOpenIn()
        {
        }

        public virtual void OnCloseIn()
        {
        }

        public virtual void OnOpenOut()
        {
        }

        public virtual void OnCloseOut()
        {
        }

        public void Setup(Id id, string prefabName)
        {
            ID = id;
            PrefabName = prefabName;
            OnInitialize();
        }

        public abstract void Open(UINode node);
        public abstract void Switch(UINode node);
        public abstract void Close(UINode node);
    }

    public class SceneNode : UINode
    {
        public override void OnInitialize()
        {
            Debug.Log("SceneNode Initialized");
        }

        public override void Open(UINode node)
        {
            if (node is WindowNode)
            {
                node.OnInitialize();
                node.OnOpenIn();
                ChildNodes.Add(node);
            }
            else
            {
                Debug.LogError("SceneNode can only open WindowNode.");
            }
        }

        public override void Switch(UINode node)
        {
            if (node is WindowNode)
            {
                if (ChildNodes.Count > 0)
                {
                    var current = ChildNodes[^1];
                    current.OnCloseOut();
                    ChildNodes.RemoveAt(ChildNodes.Count - 1);
                }

                Open(node);
            }
            else
            {
                Debug.LogError("SceneNode can only switch to WindowNode.");
            }
        }

        public override void Close(UINode node)
        {
            if (node is not WindowNode || !ChildNodes.Contains(node)) return;
            node.OnCloseOut();
            ChildNodes.Remove(node);
        }
    }

    public class WindowNode : UINode
    {
        public override void OnInitialize()
        {
            Debug.Log("WindowNode Initialized");
        }

        public override void Open(UINode node)
        {
            if (node is ScreenNode)
            {
                node.OnInitialize();
                node.OnOpenIn();
                ChildNodes.Add(node);
            }
            else
            {
                Debug.LogError("WindowNode can only open ScreenNode.");
            }
        }

        public override void Switch(UINode node)
        {
            if (node is ScreenNode)
            {
                if (ChildNodes.Count > 0)
                {
                    var current = ChildNodes[ChildNodes.Count - 1];
                    current.OnCloseOut();
                    ChildNodes.RemoveAt(ChildNodes.Count - 1);
                }

                Open(node);
            }
            else
            {
                Debug.LogError("WindowNode can only switch to ScreenNode.");
            }
        }

        public override void Close(UINode node)
        {
            if (node is not ScreenNode || !ChildNodes.Contains(node)) return;
            node.OnCloseOut();
            ChildNodes.Remove(node);
        }
    }

    public class ScreenNode : UINode
    {
        public override void OnInitialize()
        {
            Debug.Log("ScreenNode Initialized");
        }

        public override void Open(UINode node)
        {
            Debug.LogError("ScreenNode cannot open any nodes.");
        }

        public override void Switch(UINode node)
        {
            Debug.LogError("ScreenNode cannot switch nodes.");
        }

        public override void Close(UINode node)
        {
            Debug.LogError("ScreenNode cannot close any nodes.");
        }
    }
}