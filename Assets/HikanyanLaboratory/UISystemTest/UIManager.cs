using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private readonly Dictionary<string, UINode> _activeUiNodes = new();
        private readonly Queue<QueuedScreen> _screenQueue = new();
        private readonly List<UIScreen> _screenStack = new();

        public bool inputOrderFixEnabled = true;
        private Canvas _rootCanvas;
        private CanvasScaler _canvasScaler;
        private enum UIState { Ready, Processing }
        private UIState _state = UIState.Ready;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _rootCanvas = GetComponent<Canvas>();
                _canvasScaler = _rootCanvas.GetComponent<CanvasScaler>();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        
        /// <summary>
        /// UIを開く（汎用化 & ラムダ式でデータを挿入）
        /// </summary>
        public T Open<T>(string sceneId, System.Action<T> initializer = null) where T : UINode, new()
        {
            if (_activeUiNodes.ContainsKey(sceneId))
            {
                return _activeUiNodes[sceneId] as T;
            }

            // インスタンスを生成
            var node = new T();
            initializer?.Invoke(node); // 初期化処理を適用

            _activeUiNodes[sceneId] = node;
            return node;
        }
        
        /// <summary>
        /// UIを閉じる（汎用化 & ラムダ式でクローズ処理を実行）
        /// </summary>
        public bool Close<T>(string sceneId, System.Action<T> onCloseAction = null) where T : UINode
        {
            if (_activeUiNodes.TryGetValue(sceneId, out var node) && node is T typedNode)
            {
                onCloseAction?.Invoke(typedNode); // クローズ前の処理を実行
                _activeUiNodes.Remove(sceneId);
                return true;
            }
            return false;
        }

        

        /// <summary>
        /// シーンを開く
        /// </summary>
        public UIScene OpenScene(string sceneId)
        {
            if (_activeUiNodes.ContainsKey(sceneId))
                return (UIScene)_activeUiNodes[sceneId];

            var scene = new UIScene(sceneId);
            _activeUiNodes[sceneId] = scene;
            return scene;
        }
        public UIScene CloseScene(string sceneId)
        {
            if (_activeUiNodes.ContainsKey(sceneId))
            {
                _activeUiNodes.Remove(sceneId);
            }
            QueuePop(sceneId);
            return null;
        }

        /// <summary>
        /// シーンにウィンドウを追加
        /// </summary>
        public UIWindow OpenWindow(UIScene scene, string windowId)
        {
            return scene.AddWindow(windowId);
        }
        
        public UIWindow CloseWindow(UIWindow window)
        {
            if (window == null) return null;
            QueuePop(window.Id);
            return null;
        }

        /// <summary>
        /// ウィンドウにスクリーンを追加
        /// </summary>
        public UIScreen OpenScreen(UIWindow window, string screenId)
        {
            var screenNode = window.AddScreen(screenId);
            QueuePush(screenId, screenNode);
            return screenNode;
        }
        /// <summary>
        /// 画面を閉じる
        /// </summary>
        public void CloseScreen(UIScreen screenNode)
        {
            if (screenNode == null) return;
            QueuePop(screenNode.Id);
        }

        /// <summary>
        /// 画面をキューに追加（Push）
        /// </summary>
        private void QueuePush(string screenId, UIScreen screenNode)
        {
            _screenQueue.Enqueue(new QueuedScreen(screenId, screenNode, true));
            ProcessQueue();
        }

        /// <summary>
        /// 画面をキューに追加（Pop）
        /// </summary>
        private void QueuePop(string screenId)
        {
            _screenQueue.Enqueue(new QueuedScreen(screenId, null, false));
            ProcessQueue();
        }

        /// <summary>
        /// キューを処理（逐次実行）
        /// </summary>
        private void ProcessQueue()
        {
            if (_state == UIState.Processing || _screenQueue.Count == 0) return;

            _state = UIState.Processing;
            var queuedScreen = _screenQueue.Dequeue();

            if (queuedScreen.IsPush)
            {
                PushScreen(queuedScreen.ScreenId, queuedScreen.ScreenNode);
            }
            else
            {
                PopScreen(queuedScreen.ScreenId);
            }

            _state = UIState.Ready;

            if (_screenQueue.Count > 0)
            {
                ProcessQueue();
            }
        }

        /// <summary>
        /// 画面を開く
        /// </summary>
        private void PushScreen(string screenId, UIScreen screenNode)
        {
            screenNode.OnInitialize();
            screenNode.OnOpenIn();

            if (_screenStack.Count > 0)
            {
                _screenStack[0].OnCloseIn();
            }

            _screenStack.Insert(0, screenNode);
            FixInputOrder();
        }

        /// <summary>
        /// 画面を閉じる
        /// </summary>
        private void PopScreen(string screenId)
        {
            var screen = _screenStack.Find(s => s.Id == screenId);
            if (screen == null) return;

            screen.OnCloseOut();
            _screenStack.Remove(screen);

            if (_screenStack.Count > 0)
            {
                _screenStack[0].OnOpenOut();
            }
        }

        /// <summary>
        /// Unity UI の入力順序修正
        /// </summary>
        private void FixInputOrder()
        {
            if (!inputOrderFixEnabled) return;

            int order = 0;
            foreach (Transform child in _rootCanvas.transform)
            {
                var canvas = child.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = order++;
                }
            }
        }
    }

    /// <summary>
    /// 画面遷移用のキュー情報
    /// </summary>
    public class QueuedScreen
    {
        public string ScreenId { get; private set; }
        public UIScreen ScreenNode { get; private set; }
        public bool IsPush { get; private set; }

        public QueuedScreen(string screenId, UIScreen screenNode, bool isPush)
        {
            ScreenId = screenId;
            ScreenNode = screenNode;
            IsPush = isPush;
        }
    }
}
