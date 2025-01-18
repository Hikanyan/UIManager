using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HikanyanLaboratory.UI.Example
{
    public class SampleListPresenter : ListPresenterBase<SampleListView, SampleListItemModel>
    {
        [SerializeField] private SampleListView _view;
        [SerializeField] private SampleListItemModel _model;

        public Action<SampleListCellPresenter> onCellClickedCallback;

        public override async UniTask InitializeAsync(CancellationToken ct)
        {
            await base.InitializeAsync(ct);
            SetEvent();
            Debug.Log("<color=green>SampleListPresenter Initialized</color>");
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