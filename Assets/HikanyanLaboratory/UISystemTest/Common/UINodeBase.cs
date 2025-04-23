using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
{
    /// <summary>
    ///  データを格納するためのクラス
    /// </summary>
    public abstract class Parameter
    {
    }
    
    /// <summary>
    /// UIViewの基本クラス
    /// </summary>
    public abstract class UINodeBase : MonoBehaviour, IUINode
    {
        public int Id { get; private set; }
        public UINodeBase Parent { get; private set; }
        public List<IUINode> Children { get; private set; } = new();
        public bool IsActive { get; protected set; }

        public void Awake()
        {
            if (Parent == null)
            {
                Parent = transform.parent?.GetComponent<UINodeBase>();
            }
        }
        public void Start()
        {
            UIManager.Instance.RegisterNode(this);
        }

        public void OnDestroy()
        {
            UIManager.Instance.UnregisterNode(Id);
        }


        public virtual void Initialize(int id, UINodeBase parent = null)
        {
            Id = id;
            Parent = parent;
            if (parent != null)
            {
                parent.AddChild(this);
            }
        }

        public void AddChild(IUINode child)
        {
            if (Children.Contains(child)) return;
            Children.Add(child);
            SetActiveChild(child);
        }

        public void RemoveChild(IUINode child)
        {
            if (!Children.Contains(child)) return;
            Children.Remove(child);
            if (Children.Count > 0)
            {
                SetActiveChild(Children[^1]); // 最後の要素をアクティブにする
            }
        }

        public void SetActiveChild(IUINode child)
        {
            foreach (var c in Children)
            {
                ((UINodeBase)c).IsActive = false;
            }

            ((UINodeBase)child).IsActive = true;
        }

        public virtual void OnInitialize(CancellationToken cancellationToken)
        {
        }

        public virtual void OnOpenIn(CancellationToken cancellationToken)
        {
        }

        public virtual void OnCloseIn(CancellationToken cancellationToken)
        {
        }

        public virtual void OnOpenOut(CancellationToken cancellationToken)
        {
        }

        public virtual void OnCloseOut(CancellationToken cancellationToken)
        {
        }
    }
    
    public abstract class UINodeBase<T> : UINodeBase where T : Parameter, new()
    {
        public T Parameter { get; private set; }

        public virtual void Initialize(int id, UINodeBase parent = null, T parameter = null)
        {
            base.Initialize(id, parent);
            Parameter = parameter ?? new T();
        }
    }
    
}