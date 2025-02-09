namespace HikanyanLaboratory.UISystemTest.Example
{
    public class MainWindow : UIWindow
    {
        public MainWindow(UIScene parent) : base("MainWindow", parent) { }

        public Screen1 OpenScreen1()
        {
            return AddScreen("Screen1") as Screen1;
        }

        public Screen2 OpenScreen2()
        {
            return AddScreen("Screen2") as Screen2;
        }
    }
}