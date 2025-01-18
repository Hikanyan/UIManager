using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace HikanyanLaboratory.UI.Example
{
    public class SampleListView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _tmpText;

        public void Awake()
        {
            _tmpText.text = "Hello World!";
        }
    }
}