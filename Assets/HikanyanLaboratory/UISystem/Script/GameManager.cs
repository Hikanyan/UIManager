using HikanyanLaboratory.UI.Example;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class GameManager : MonoBehaviour
    {
        [PrefabReference(PrefabKeys.SampleList)]
        private GameObject _playerPrefab;

        private void Start()
        {
            UIManager.Instance.OpenPresenter(PrefabKeys.SampleList);
            // UIManager.Instance.OpenPresenter<SampleListPresenter>(PrefabKeys.SampleList,data=>);

            UIManager.Instance.ClosePresenter(PrefabKeys.SampleList);
        }
    }
}