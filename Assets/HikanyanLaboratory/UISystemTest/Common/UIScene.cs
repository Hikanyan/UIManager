namespace HikanyanLaboratory.UISystemTest
{
    public class UIScene : UINode
    {
        public UIScene(string id) : base(id, null) { }

        /// <summary>
        /// シーンにウィンドウを追加
        /// </summary>
        public UIWindow AddWindow(string id)
        {
            var window = new UIWindow(id, this);
            AddChild(window);
            return window;
        }
    }
}