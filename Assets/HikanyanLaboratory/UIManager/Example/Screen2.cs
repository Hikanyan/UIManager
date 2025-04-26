using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HikanyanLaboratory.UISystem.Example
{
    public class Screen2 : UIScreen<Screen2.Screen2Parameter>
    {
        public class Screen2Parameter : Parameter
        {
            public int Value;
        }

        [SerializeField] private TMP_Text _valueText;
        [SerializeField] private Button _switchButton;

        public override UniTask OnInitialize(CancellationToken cancellationToken)
        {
            Debug.Log($"[Screen2] Initialized. Received value = {Parameter.Value}");

            _valueText.text = Parameter.Value.ToString();
            _switchButton.onClick.AddListener(() => { SwitchToScreen1().Forget(); });

            return UniTask.CompletedTask;
        }

        private async UniTaskVoid SwitchToScreen1()
        {
            var parameter = new Screen1.Screen1Parameter { Value = 42 };

            await UIManager.Instance.OpenAsync<Screen1, Screen1.Screen1Parameter>(
                PrefabKeys.Screen1,
                parameter,
                Parent,
                CancellationToken.None);

            UIManager.Instance.Close(GetInstanceID(), CancellationToken.None);
        }
    }
}