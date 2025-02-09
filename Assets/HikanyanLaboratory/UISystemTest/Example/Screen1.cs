using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest.Example
{
    public class Screen1 : UIScreen
    {
        public Button switchButton;

        public Screen1(UIWindow parent) : base("Screen1", parent) { }

        public override void OnInitialize()
        {
            Debug.Log("[Screen1] Initialized");

            switchButton = GameObject.Find("SwitchButton").GetComponent<Button>();
            switchButton.onClick.AddListener(SwitchToScreen2);
        }

        public override void OnOpenIn()
        {
            Debug.Log("[Screen1] Opened");
            gameObject.SetActive(true);
        }

        public override void OnCloseOut()
        {
            Debug.Log("[Screen1] Closed");
            gameObject.SetActive(false);
        }

        private void SwitchToScreen2()
        {
            Debug.Log("[Screen1] Switching to Screen2");
            var window = Parent as MainWindow;
            window?.OpenScreen2();
        }
    }
}