using Cysharp.Threading.Tasks;
using System.Threading;
using HikanyanLaboratory.UI;

namespace HikanyanLaboratory.UISystem
{
    public class RankingWindowPresenter : PresenterBase<RankingWindowView, RankingWindowModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}
