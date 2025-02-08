using System.Collections.Generic;
using HikanyanLaboratory.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private UINode _rootNode;
        private readonly Dictionary<string, Screen> _screenCache = new();
        private Queue<QueuedScreen> _screenQueue = new();
        private readonly List<Screen> _screenStack = new();

        public bool inputOrderFixEnabled = true;
        private Canvas _rootCanvas;
        private CanvasScaler _canvasScaler;

        private enum UIState
        {
            Ready,
            Push,
            Pop
        }

        private UIState _state = UIState.Ready;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _rootNode = new UINode("Root");
                _rootCanvas = GetComponent<Canvas>();
                _canvasScaler = _rootCanvas.GetComponent<CanvasScaler>();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 画面を開く（キューに追加）
        /// </summary>
        public UINode Open(string id, UINode parent = null)
        {
            parent ??= _rootNode;
            var newNode = new UINode(id, parent);

            // 画面を取得または生成
            if (!_screenCache.TryGetValue(id, out var screen))
            {
                screen = LoadScreen(id);
                _screenCache[id] = screen;
            }

            newNode.SetScreen(screen);

            screen.gameObject.SetActive(true);
            screen.OnInitialize();
            screen.OnOpenIn();

            // 既存のトップ画面にフォーカスを失わせる
            if (_screenStack.Count > 0)
            {
                _screenStack[0].OnCloseIn();
            }

            _screenStack.Insert(0, screen);
            FixInputOrder();

            return newNode;
        }

        /// <summary>
        /// 画面を閉じる（キューに追加）
        /// </summary>
        public void Close(UINode node)
        {
            if (node == null) return;

            if (_screenCache.TryGetValue(node.Id, out var screen))
            {
                screen.OnCloseOut();
                screen.gameObject.SetActive(false);
                _screenCache.Remove(node.Id);
            }

            if (node.Parent != null)
            {
                node.Parent.RemoveChild(node);
            }

            // 新しいトップ画面にフォーカスを戻す
            if (_screenStack.Count > 0)
            {
                _screenStack[0].OnOpenOut();
            }
        }

        /// <summary>
        /// 画面を切り替える
        /// </summary>
        public void Switch(UINode parent, string newId)
        {
            var newNode = new UINode(newId, parent);

            if (_screenCache.TryGetValue(parent.Id, out var oldScreen))
            {
                oldScreen.OnCloseIn();
                oldScreen.gameObject.SetActive(false);
            }

            if (!_screenCache.TryGetValue(newId, out var newScreen))
            {
                newScreen = LoadScreen(newId);
                _screenCache[newId] = newScreen;
            }

            newNode.SetScreen(newScreen);

            newScreen.gameObject.SetActive(true);
            newScreen.OnOpenOut();

            parent.SetActiveChild(newNode);
            FixInputOrder();
        }

        /// <summary>
        /// 指定したIDの画面をプレハブからロード
        /// </summary>
        private Screen LoadScreen(string id)
        {
            string path = PrefabKeys.GetPrefabPath(id);
            var prefab = Resources.Load<Screen>(path);
            return Instantiate(prefab, _rootCanvas.transform);
        }

        /// <summary>
        /// Unity の UI の入力順序の問題に対処する
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
}

/// <summary>
/// キュー内での画面情報を管理するクラス
/// </summary>
public class QueuedScreen
{
    public string Id { get; private set; }
    public UINode Parent { get; private set; }
    public bool IsPush { get; private set; }

    public QueuedScreen(string id, UINode parent, bool isPush)
    {
        Id = id;
        Parent = parent;
        IsPush = isPush;
    }
}
