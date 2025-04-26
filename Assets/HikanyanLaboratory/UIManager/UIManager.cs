using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystem
{
    public class UIManager : MonoBehaviour
    {
        public class Handler<T> : IDisposable where T : IUINode
        {
            public readonly int UniqueId;
            public readonly UIManager Manager;

            public Handler(int uniqueId, UIManager manager)
            {
                UniqueId = uniqueId;
                Manager = manager;
            }

            public async UniTask WaitCloseAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await UniTask.WaitWhile(() => Manager._activeUiNodes.ContainsKey(UniqueId), cancellationToken: cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    if (Manager._activeUiNodes.ContainsKey(UniqueId))
                    {
                        await Manager.CloseAsync(UniqueId, default);
                    }
                }
            }

            public void Dispose()
            {
                Manager?.Close(UniqueId, CancellationToken.None);
            }
        }

        public static UIManager Instance { get; private set; }

        private readonly Dictionary<int, IUINode> _activeUiNodes = new();
        private readonly List<IUINode> _uiStack = new();
        [SerializeField] private Canvas _rootCanvas;
        private readonly bool _inputOrderFixEnabled = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public async UniTask<IUINode> RegisterNode(UINodeBase node, CancellationToken cancellationToken = default)
        {
            if (node == null) return null;
            int id = node.GetInstanceID();
            node.Initialize(id, node.Parent);
            _activeUiNodes.TryAdd(id, node);
            await PushNode(node, cancellationToken);
            return node;
        }


        public async void UnregisterNode(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_activeUiNodes.Remove(id, out var node)) return;

                await PopNode(node, cancellationToken);

                if (node is MonoBehaviour monoBehaviour)
                {
                    Destroy(monoBehaviour.gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        public T Open<T>(string prefabKey, UINodeBase parent = null, CancellationToken cancellationToken = default)
            where T : UINodeBase
        {
            var node = UINodeFactory.Create<T>(prefabKey, parent);
            if (node == null) return null;

            if (parent == null)
            {
                node.transform.SetParent(_rootCanvas.transform, false);
            }

            return node;
        }

        public async UniTask<Handler<T>> OpenAsync<T, TParam>(
            string prefabKey,
            TParam parameter,
            UINodeBase parent = null,
            CancellationToken cancellationToken = default)
            where T : UINodeBase<TParam>
            where TParam : Parameter, new()
        {
            var node = UINodeFactory.Create<T>(prefabKey, parent);
            if (node == null) return null;

            if (parent == null)
            {
                node.transform.SetParent(_rootCanvas.transform, false);
            }

            node.SetParameter(parameter, cancellationToken);
            var handler = new Handler<T>(node.Id, this);
            return handler;
        }


        public void Close(int uniqueId, CancellationToken cancellationToken)
        {
            if (_activeUiNodes.ContainsKey(uniqueId))
            {
                UnregisterNode(uniqueId, cancellationToken);
            }
        }

        public async UniTask CloseAsync(int uniqueId, CancellationToken cancellationToken)
        {
            if (_activeUiNodes.TryGetValue(uniqueId, out var node))
            { 
                await PopNode(node, cancellationToken);
                UnregisterNode(uniqueId, cancellationToken);
            }
        }
        private async UniTask PushNode(IUINode uiNode, CancellationToken cancellationToken)
        {
            try
            {
                await uiNode.OnInitialize(cancellationToken);
                await uiNode.OnOpenIn(cancellationToken);
                await uiNode.OnOpenOut(cancellationToken);
                _uiStack.Insert(0, uiNode);
                FixInputOrder();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }

        private async UniTask PopNode(IUINode uiNode, CancellationToken cancellationToken)
        {
            try
            {
                if (!_uiStack.Contains(uiNode)) return;
                await uiNode.OnCloseIn(cancellationToken);
                await uiNode.OnCloseOut(cancellationToken);
                _uiStack.Remove(uiNode);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
        }


        public void BringToFront(IUINode node)
        {
            if (!_uiStack.Contains(node)) return;

            _uiStack.Remove(node);
            _uiStack.Insert(0, node);
            FixInputOrder();
        }

        private void FixInputOrder()
        {
            if (!_inputOrderFixEnabled) return;

            int order = 0;
            foreach (Transform child in _rootCanvas.transform)
            {
                var canvas = child.GetComponent<Canvas>();
                if (canvas == null) continue;
                canvas.overrideSorting = true;
                canvas.sortingOrder = order++;
            }
        }
    }
}
