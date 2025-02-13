using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest.Example
{
    public class Screen2 : UIScreen
    {
        public Button _switchButton;

        public override void OnInitialize()
        {
            Debug.Log("[Screen2] Initialized");

            _switchButton = GameObject.Find("SwitchButton").GetComponent<Button>();
            _switchButton.onClick.AddListener(SwitchToScreen1);
        }

        private void SwitchToScreen1()
        {
            Debug.Log("[Screen2] Switching to Screen1");
        }
    }
}