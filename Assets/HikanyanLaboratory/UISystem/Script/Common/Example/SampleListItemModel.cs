using System;
using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.UI.Example
{
    [Serializable]
    public class SampleListItemModel
    {
        [SerializeField] private List<GameObject> _prefabList = new List<GameObject>();
        
    }
}