using Cysharp.Threading.Tasks;
        using System.Threading;
        using HikanyanLaboratory.UI;

        namespace HikanyanLaboratory.UISystem
{
    public class TitleWindowWindowPresenter : PresenterBase<TitleWindowWindowView, TitleWindowWindowModel>
    {
        public override UniTask InitializeAsync(CancellationToken ct)
        {
            return default;
        }
    }
}