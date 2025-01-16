using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class ExampleUsage : MonoBehaviour
    {
        [PrefabReference(PrefabKeys.Cube)]
        private GameObject _playerPrefab;

        private void Awake()
        {
            _playerPrefab = PrefabLoader.GetPrefab(PrefabKeys.Cube);
            Debug.Log(_playerPrefab.name);
            Instantiate(_playerPrefab);
        }
    }
}