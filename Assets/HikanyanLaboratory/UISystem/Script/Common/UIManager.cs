using System.Collections.Generic;
using UnityEngine;

namespace HikanyanLaboratory.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private readonly Dictionary<string, PresenterBase> _presenters = new Dictionary<string, PresenterBase>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void RegisterPresenter(string key, PresenterBase presenter)
        {
            _presenters.TryAdd(key, presenter);
        }

        public void OpenPresenter(string key)
        {
            if (_presenters.TryGetValue(key, out var presenter))
            {
                presenter.Open();
            }
        }

        public void ClosePresenter(string key)
        {
            if (_presenters.TryGetValue(key, out var presenter))
            {
                presenter.Close();
            }
        }
    }
}