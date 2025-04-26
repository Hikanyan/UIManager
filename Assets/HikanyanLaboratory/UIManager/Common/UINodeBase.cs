using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HikanyanLaboratory.UISystem
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
            _ = UIManager.Instance.RegisterNode(this);
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

        public virtual async UniTask OnInitialize(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnOpenIn(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnCloseIn(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnOpenOut(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }

        public virtual async UniTask OnCloseOut(CancellationToken cancellationToken)
        {
            await UniTask.CompletedTask;
        }
    }
    
    public abstract class UINodeBase<T> : UINodeBase where T : Parameter, new()
    {
        public T Parameter { get; private set; }

        public virtual void SetParameter(T parameter, CancellationToken cancellationToken)
        {
            Parameter = parameter ?? new T();
        }

        public override async UniTask OnInitialize(CancellationToken cancellationToken)
        {
            await base.OnInitialize(cancellationToken);
            // 追加のパラメータ初期化処理はここでオーバーライドされることを想定
        }
    }

}