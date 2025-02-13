using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private readonly Dictionary<string, IUINode> _activeUiNodes = new();
        private readonly List<IUINode> _uiStack = new();
        [SerializeField] private Canvas _rootCanvas;
        private bool _inputOrderFixEnabled = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _rootCanvas = GetComponent<Canvas>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// UIを開く（既存のものがあれば最前面に移動）
        /// </summary>
        public T Open<T>(string prefabKey, IUINode parent = null) where T : UINodeBase
        {
            // すでに開いているものがあれば最前面に移動
            foreach (var uiNode in _activeUiNodes.Values)
            {
                if (uiNode is T existingNode)
                {
                    BringToFront(existingNode);
                    return existingNode;
                }
            }

            // ユニークIDを生成
            string uniqueId = Guid.NewGuid().ToString();

            // UINodeFactory でインスタンスを作成
            var node = UINodeFactory.Create<T>(uniqueId, prefabKey, parent);
            if (node == null) return null;

            // UI ノードを rootCanvas の下に配置
            if (parent == null)
            {
                node.transform.SetParent(_rootCanvas.transform, false);
            }

            // アクティブな UI に追加
            _activeUiNodes[uniqueId] = node;

            // 画面を開く（Push）
            PushNode(node);

            return node;
        }

        /// <summary>
        /// UIを閉じる
        /// </summary>
        public void Close<T>() where T : UINodeBase
        {
            string id = _activeUiNodes.FirstOrDefault(x => x.Value is T).Key;
            if (!_activeUiNodes.TryGetValue(id, out var node) || node is not T typedNode) return;

            PopNode(typedNode);
            _activeUiNodes.Remove(id);
        }

        /// <summary>
        /// 画面を開く（Push）
        /// </summary>
        private void PushNode(IUINode uiNode)
        {
            uiNode.OnInitialize();
            uiNode.OnOpenIn();

            if (_uiStack.Count > 0)
            {
                _uiStack[0].OnCloseIn();
            }

            _uiStack.Insert(0, uiNode);
            FixInputOrder();
        }

        /// <summary>
        /// 画面を閉じる（Pop）
        /// </summary>
        private void PopNode(IUINode uiNode)
        {
            if (!_uiStack.Contains(uiNode)) return;

            uiNode.OnCloseOut();
            _uiStack.Remove(uiNode);

            if (_uiStack.Count > 0)
            {
                _uiStack[0].OnOpenOut();
            }
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