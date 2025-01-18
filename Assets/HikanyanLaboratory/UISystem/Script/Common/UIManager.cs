using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private Canvas _rootCanvas;

        public static UIManager Instance { get; private set; }

        private readonly Dictionary<string, UINode> _presenters = new Dictionary<string, UINode>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                AutoRegisterPrefabs();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// PrefabKeysからプレゼンターを自動登録する
        /// </summary>
        private void AutoRegisterPrefabs()
        {
            foreach (var key in PrefabKeys.GetAllKeys())
            {
                GameObject prefab = PrefabLoader.GetPrefab(key);
                if (prefab != null)
                {
                    UINode node = Instantiate(prefab, _rootCanvas.transform).GetComponent<UINode>();
                    RegisterPresenter(key, node);
                }
            }
        }

        public void RegisterPresenter(string key, UINode presenter)
        {
            _presenters.TryAdd(key, presenter);
        }

        public void OpenPresenter(string key)
        {
            if (_presenters.TryGetValue(key, out var presenter))
            {
                presenter.OnInitialize();
                presenter.OnOpenIn();
            }
        }

        public void ClosePresenter(string key)
        {
            if (_presenters.TryGetValue(key, out var presenter))
            {
                presenter.OnCloseOut();
            }
        }
    }
}