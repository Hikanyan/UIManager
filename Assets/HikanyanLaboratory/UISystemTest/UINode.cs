using System.Collections.Generic;

namespace HikanyanLaboratory.UISystemTest
{
    public class UINode
    {
        public string Id { get; private set; }
        public UINode Parent { get; private set; }
        public List<UINode> Children { get; private set; }
        public bool IsActive { get; private set; }
        private Screen _screen;

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

        /// <summary>
        /// ノードに対応する `Screen` を設定
        /// </summary>
        public void SetScreen(Screen screen)
        {
            _screen = screen;
            screen.Setup(Id);
        }

        /// <summary>
        /// 子ノードを追加し、アクティブにする
        /// </summary>
        public void AddChild(UINode child)
        {
            if (!Children.Contains(child))
            {
                Children.Add(child);
                SetActiveChild(child);
            }
        }

        /// <summary>
        /// 子ノードを削除
        /// </summary>
        public void RemoveChild(UINode child)
        {
            if (Children.Contains(child))
            {
                Children.Remove(child);
                if (Children.Count > 0)
                {
                    SetActiveChild(Children[^1]); // 最後のノードをアクティブに
                }
            }
        }

        /// <summary>
        /// アクティブなノードを設定
        /// </summary>
        public void SetActiveChild(UINode child)
        {
            foreach (var c in Children)
            {
                c.IsActive = false;
            }

            child.IsActive = true;

            // UI画面がある場合、ライフサイクルイベントを実行
            if (_screen != null)
            {
                _screen.OnCloseIn();
            }

            if (child._screen != null)
            {
                child._screen.OnOpenOut();
            }
        }

        /// <summary>
        /// ノードを開く処理
        /// </summary>
        public void Open()
        {
            if (_screen != null)
            {
                _screen.OnInitialize();
                _screen.OnOpenIn();
            }

            IsActive = true;
        }

        /// <summary>
        /// ノードを閉じる処理
        /// </summary>
        public void Close()
        {
            if (_screen != null)
            {
                _screen.OnCloseOut();
                _screen.gameObject.SetActive(false);
            }

            Parent?.RemoveChild(this);
            IsActive = false;
        }
    }
}