namespace HikanyanLaboratory.UISystemTest.Example
{
    public class MainScene : UIScene
    {
        public MainScene() : base("MainScene") { }

        public MainWindow OpenMainWindow()
        {
            return AddWindow("MainWindow") as MainWindow;
        }
    }
}