using Cysharp.Threading.Tasks;
using System.Threading;
using HikanyanLaboratory.UI;

namespace HikanyanLaboratory.UISystem
{
    public class TestWindowPresenter : PresenterBase<TestWindowView, TestWindowModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}
