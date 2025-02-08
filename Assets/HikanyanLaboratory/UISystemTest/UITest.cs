namespace HikanyanLaboratory.UISystemTest
{
    using UnityEngine;

    public class UITest : MonoBehaviour
    {
        private UIManager uiManager;
        private UINode sceneNode;
        private UINode windowNode;
        private UINode screenNode1;
        private UINode screenNode2;

        private void Start()
        {
            uiManager = UIManager.Instance;

            Debug.Log("✅ シーン（Scene）を開く");
            sceneNode = uiManager.Open(PrefabKeys.Cube);

            Debug.Log("✅ ウィンドウ（Window）を開く");
            windowNode = uiManager.Open("MainWindow", sceneNode);

            Debug.Log("✅ 画面1（Screen1）を開く");
            screenNode1 = uiManager.Open("Screen1", windowNode);

            Debug.Log("✅ 画面2（Screen2）を開く（画面1を隠す）");
            screenNode2 = uiManager.Open("Screen2", windowNode);

            Debug.Log("✅ 画面2を閉じる（画面1が再びアクティブに）");
            uiManager.Close(screenNode2);

            Debug.Log("✅ ウィンドウを閉じる（シーンだけが残る）");
            uiManager.Close(windowNode);
        }
    }
}