﻿using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystemTest.Example
{
    public class Screen1 : UIScreen
    {
        public Button _switchButton;

        public override void OnInitialize()
        {
            Debug.Log("[Screen1] Initialized");

            _switchButton = GameObject.Find("SwitchButton").GetComponent<Button>();
            _switchButton.onClick.AddListener(SwitchToScreen2);
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
            UIManager.Instance.Open<Screen2>(PrefabKeys.Cube,this);
            UIManager.Instance.Close<Screen1>();
        }
    }
}