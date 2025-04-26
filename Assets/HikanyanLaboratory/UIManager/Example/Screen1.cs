using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystem.Example
{
    public class Screen1 : UIScreen<Screen1.Screen1Parameter>
    {
        public class Screen1Parameter : Parameter
        {
            public int Value;
        }

        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Button _switchButton;

        public override UniTask OnInitialize(CancellationToken cancellationToken)
        {
            Debug.Log("[Screen1] Initialized");

            _valueText.text = Parameter.Value.ToString();
            _switchButton.onClick.AddListener(() => { SwitchToScreen2().Forget(); });

            return UniTask.CompletedTask;
        }

        public override UniTask OnOpenIn(CancellationToken cancellationToken)
        {
            Debug.Log("[Screen1] Opened");
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public override UniTask OnCloseOut(CancellationToken cancellationToken)
        {
            Debug.Log("[Screen1] Closed");
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid SwitchToScreen2()
        {
            var parameter = new Screen2.Screen2Parameter { Value = 999 };

            var handler = await UIManager.Instance.OpenAsync<Screen2, Screen2.Screen2Parameter>(
                PrefabKeys.Screen2,
                parameter,
                Parent,
                CancellationToken.None);

            Debug.Log("[Screen1] Opened Screen2 with value: " + parameter.Value);

            UIManager.Instance.Close(GetInstanceID(), CancellationToken.None);
        }
    }
}