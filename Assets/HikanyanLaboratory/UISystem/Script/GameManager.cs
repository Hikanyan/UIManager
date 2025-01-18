using HikanyanLaboratory.UI.Example;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class GameManager : MonoBehaviour
    {
        [PrefabReference(PrefabKeys.Cube)] private GameObject _playerPrefab;
        private void Start()
        {
            UIManager.Instance.OpenPresenter(PrefabKeys.Cube);
        }
    }
}