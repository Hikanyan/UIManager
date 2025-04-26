using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HikanyanLaboratory.UISystem.Example;

namespace HikanyanLaboratory.UISystem
{
    public class UITest : MonoBehaviour
    {
        private async UniTaskVoid Start()
        {
            var uiManager = UIManager.Instance;

            // シーンとウィンドウを開く
            var scene = uiManager.Open<MainScene>(PrefabKeys.MainScene);
            var window = uiManager.Open<MainWindow>(PrefabKeys.MainWindow, scene);

            // Screen1 を開いてパラメータを渡す
            var param = new Screen1.Screen1Parameter { Value = 2025 };

            await uiManager.OpenAsync<Screen1, Screen1.Screen1Parameter>(
                PrefabKeys.Screen1,
                param,
                window,
                CancellationToken.None);
        }
    }
}