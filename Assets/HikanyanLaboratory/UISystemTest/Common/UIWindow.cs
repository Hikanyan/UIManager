namespace HikanyanLaboratory.UISystemTest
{
    public class UIWindow : UINode
    {
        public UIWindow(string id, UIScene parent) : base(id, parent) { }

        /// <summary>
        /// ウィンドウにスクリーンを追加
        /// </summary>
        public UIScreen AddScreen(string id)
        {
            var screen = new UIScreen(id, this);
            AddChild(screen);
            return screen;
        }
    }
}