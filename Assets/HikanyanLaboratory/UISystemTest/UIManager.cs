using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest
{
    public class UIManager : MonoBehaviour
    {
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

        /// <summary>
        /// UI ノードを `UIManager` に登録
        /// </summary>
        public void RegisterNode(UINodeBase node, CancellationToken cancellationToken = default)
        {
            if (node == null) return;
            _activeUiNodes.TryAdd(node.Id, node);
            PushNode(node, cancellationToken);
        }


        /// <summary>
        /// UI ノードを `UIManager` から削除
        /// </summary>
        public void UnregisterNode(int id, CancellationToken cancellationToken = default)
        {
            if (!_activeUiNodes.Remove(id, out var node)) return;
            PopNode(node, cancellationToken);
        }


        /// <summary>
        /// UIを開く（既存のものがあれば最前面に移動）
        /// </summary>
        public T Open<T>(string prefabKey, UINodeBase parent = null)
            where T : UINodeBase
        {
            // UINodeFactory でインスタンスを作成
            var node = UINodeFactory.Create<T>(prefabKey, parent);
            if (node == null) return null;
            // UI ノードを rootCanvas の下に配置
            if (parent == null)
            {
                node.transform.SetParent(_rootCanvas.transform, false);
            }

            // アクティブな UI に追加
            // RegisterNode(node);
            return node;
        }


        /// <summary>
        /// UIを閉じる（UINodeBaseのID指定で閉じる）
        /// </summary>
        public void Close(int uniqueId, CancellationToken cancellationToken)
        {
            var closeTarget = _activeUiNodes[uniqueId];
            if (closeTarget == null) return;

            UnregisterNode(uniqueId);
        }


        /// <summary>
        /// 画面を開く（Push）
        /// </summary>
        private void PushNode(IUINode uiNode, CancellationToken cancellationToken)
        {
            uiNode.OnInitialize(cancellationToken);
            uiNode.OnOpenIn(cancellationToken);
            uiNode.OnOpenOut(cancellationToken);
            _uiStack.Insert(0, uiNode);
            FixInputOrder();
        }


        /// <summary>
        /// 画面を閉じる（Pop）
        /// </summary>
        private void PopNode(IUINode uiNode, CancellationToken cancellationToken)
        {
            if (!_uiStack.Contains(uiNode)) return;
            uiNode.OnCloseIn(cancellationToken);
            uiNode.OnCloseOut(cancellationToken);
            _uiStack.Remove(uiNode);
        }


        /// <summary>
        /// 画面を最前面に移動
        /// </summary>
        private void BringToFront(IUINode node)
        {
            if (!_uiStack.Contains(node)) return;

            _uiStack.Remove(node);
            _uiStack.Insert(0, node);
            FixInputOrder();
        }

        /// <summary>
        /// Unity UI の入力順序修正
        /// </summary>
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