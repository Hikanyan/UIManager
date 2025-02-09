using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest.Example
{
    public class Screen2 : UIScreen
    {
        public Button switchButton;

        public Screen2(UIWindow parent) : base("Screen2", parent) { }

        public override void OnInitialize()
        {
            Debug.Log("[Screen2] Initialized");

            switchButton = GameObject.Find("SwitchButton").GetComponent<Button>();
            switchButton.onClick.AddListener(SwitchToScreen1);
        }

        // public override void OnOpenIn()
        // {
        //     Debug.Log("[Screen2] Opened");
        //     gameObject.SetActive(true);
        // }
        //
        // public override void OnCloseOut()
        // {
        //     Debug.Log("[Screen2] Closed");
        //     gameObject.SetActive(false);
        // }

        private void SwitchToScreen1()
        {
            Debug.Log("[Screen2] Switching to Screen1");
            var window = Parent as MainWindow;
            window?.OpenScreen1();
        }
    }
}