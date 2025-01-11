// using UnityEngine.UI;
//
// namespace HikanyanLaboratory.UI.Example
// {
//     public class ExampleMenuScreen : UINode
//     {
//         public Text headerLabel;
//         public Button buttonA;
//
//         protected override void OnInitialize()
//         {
//             buttonA.onClick.AddListener(HandleButtonAClicked);
//         }
//
//         protected override void OnOpenIn()
//         {
//             headerLabel.text = "Click on a button...";
//         }
//
//         private void HandleButtonAClicked()
//         {
//             UIManager.Instance.OpenPresenter("ExamplePopupScreen");
//         }
//     }
// }