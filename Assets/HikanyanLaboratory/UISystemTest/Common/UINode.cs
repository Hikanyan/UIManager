using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
{
    public class UINode : MonoBehaviour
    {
        public string Id { get; private set; }
        public UINode Parent { get; private set; }
        public List<UINode> Children { get; private set; }
        public bool IsActive { get; protected set; }

        public UINode(string id, UINode parent = null)
        {
            Id = id;
            Parent = parent;
            Children = new List<UINode>();

            if (parent != null)
            {
                parent.AddChild(this);
            }
        }

        public void AddChild(UINode child)
        {
            if (!Children.Contains(child))
            {
                Children.Add(child);
                SetActiveChild(child);
            }
        }

        public void RemoveChild(UINode child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
                if (Children.Count > 0)
                {
                    SetActiveChild(Children[Children.Count - 1]);
                }
            }
        }

        public void SetActiveChild(UINode child)
        {
            foreach (var c in Children)
            {
                c.IsActive = false;
            }

            child.IsActive = true;
        }


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
    }
}