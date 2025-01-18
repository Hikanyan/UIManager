using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public abstract class PresenterBase<TView, TModel> : ScreenNode
        where TView : MonoBehaviour
    {
        protected TView View;
        protected TModel Model;

        // ViewとModelのセット
        public void SetViewAndModel(TView view, TModel model)
        {
            View = view;
            Model = model;
        }

        public override void OnInitialize()
        {
            base.OnInitialize();
            InitializeAsync(CancellationToken.None).Forget();
        }

        // 初期化処理
        public virtual async UniTask InitializeAsync(CancellationToken ct)
        {
            await UniTask.Yield();
            Debug.Log($"{GetType().Name} Initialized");
        }

        // 開く処理
        public virtual void Open()
        {
            View.gameObject.SetActive(true);
            Debug.Log($"{GetType().Name} Opened");
        }

        // 閉じる処理
        public virtual void Close()
        {
            View.gameObject.SetActive(false);
            Debug.Log($"{GetType().Name} Closed");
        }
    }
}