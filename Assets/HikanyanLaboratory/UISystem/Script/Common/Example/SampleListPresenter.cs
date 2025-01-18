using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace HikanyanLaboratory.UI.Example
{
    public class SampleListPresenter:ListPresenterBase<SampleListView, SampleListItemModel>
    {
        public Action<SampleListCellPresenter> onCellClickedCallback;

        public override async UniTask InitializeAsync(CancellationToken ct)
        {
            await base.InitializeAsync(ct);
            SetEvent();
        }

        private void SetEvent()
        {
            Action<SampleListCellPresenter> action = (cell) => onCellClickedCallback?.Invoke(cell);
            foreach (var cell in AllCells)
            {
                if (cell is SampleListCellPresenter sampleCell)
                {
                    sampleCell.OnClickedCallback += action;
                }
            }
        }
    }
}