using Cysharp.Threading.Tasks;
using System.Threading;
using HikanyanLaboratory.UI;

namespace HikanyanLaboratory.UISystem
{
    public class MainWindowWindowPresenter : PresenterBase<MainWindowWindowView, MainWindowWindowModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}