using Cysharp.Threading.Tasks;
using System.Threading;
using HikanyanLaboratory.UI;

namespace HikanyanLaboratory.UISystem
{
    public class MainWindowPresenter : PresenterBase<MainWindowView, MainWindowModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}
