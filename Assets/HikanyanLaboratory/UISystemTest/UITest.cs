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

            Debug.Log("✅ シーンを開く");
            _scene = _uiManager.OpenScene(PrefabKeys.MainScene);

            Debug.Log("✅ ウィンドウを開く");
            _window = _uiManager.OpenWindow(_scene, PrefabKeys.MainWindow);

            Debug.Log("✅ 画面1を開く");
            _screen1 = _uiManager.OpenScreen(_window, PrefabKeys.Screen1);

            Debug.Log("✅ 画面2を開く（画面1を隠す）");
            _screen2 = _uiManager.OpenScreen(_window, PrefabKeys.Screen2);

            Debug.Log("✅ 画面2を閉じる（画面1が再びアクティブに）");
            _uiManager.CloseScreen(_screen2);

            Debug.Log("✅ ウィンドウを閉じる（シーンだけが残る）");
            _uiManager.CloseWindow(_window);
        }
    }
}