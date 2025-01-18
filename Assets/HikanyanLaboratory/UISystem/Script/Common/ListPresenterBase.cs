using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public abstract class ListPresenterBase<TView, TModel> : ScreenNode
    {
        protected TView View;
        protected List<TModel> ModelList;
        protected List<UINode> AllCells = new List<UINode>();


        public override void OnInitialize()
        {
            base.OnInitialize();
            InitializeAsync(CancellationToken.None).Forget();
        }

        public virtual async UniTask InitializeAsync(CancellationToken ct)
        {
            await UniTask.Yield();
            Debug.Log("ListPresenterBase Initialized");
        }
    }
}