using HikanyanLaboratory.UI.Example;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class GameManager : MonoBehaviour
    {
        private void Start()
        {
            //UIManager.Instance.RegisterPresenter("ExampleMenuScreen", ExampleMenuScreen);
            //UIManager.Instance.RegisterPresenter("ExamplePopupScreen", ExamplePopupScreenUI);

            UIManager.Instance.OpenPresenter("ExampleMenuScreen");
        }
    }
}