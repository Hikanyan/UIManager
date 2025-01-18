// using UnityEngine.UI;
//
// namespace HikanyanLaboratory.UI.Example
// {
//     public class ExamplePopupScreenUI : ScreenUINode
//     {
//         public Text messageLabel;
//         public Button okButton;
//
//         protected override void OnInitialize()
//         {
//             okButton.onClick.AddListener(HandleOkClicked);
//         }
//
//         protected override void OnOpenIn()
//         {
//             messageLabel.text = "You clicked a button, good job!";
//         }
//
//         private void HandleOkClicked()
//         {
//             UIManager.Instance.ClosePresenter("ExamplePopupScreen");
//         }
//     }
// }