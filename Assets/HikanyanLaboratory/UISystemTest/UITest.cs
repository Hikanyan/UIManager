using HikanyanLaboratory.UISystemTest.Example;
using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
{
    public class UITest : MonoBehaviour
    {
        private UIManager _uiManager;
        private UIScene _scene;
        private UIWindow _window;
        private UIScreen _screen1;
        private UIScreen _screen2;

        private void Start()
        {
            _uiManager = UIManager.Instance;

            _uiManager.Open<MainWindow>(PrefabKeys.Cube);
        }
    }
}