
using Cysharp.Threading.Tasks;
using System.Threading;
using HikanyanLaboratory.UI;

namespace HikanyanLaboratory.UISystem
{
    public class PopupScreenScreenPresenter : PresenterBase<PopupScreenScreenView, PopupScreenScreenModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}