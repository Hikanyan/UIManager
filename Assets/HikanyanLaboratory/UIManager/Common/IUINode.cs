using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace HikanyanLaboratory.UISystem
{
    public interface IUINode
    {
        int Id { get; }
        UINodeBase Parent { get; }
        List<IUINode> Children { get; }
        bool IsActive { get; }

        void AddChild(IUINode child);
        void RemoveChild(IUINode child);
        void SetActiveChild(IUINode child);

        UniTask OnInitialize(CancellationToken cancellationToken);
        UniTask OnOpenIn(CancellationToken cancellationToken);
        UniTask OnCloseIn(CancellationToken cancellationToken);
        UniTask OnOpenOut(CancellationToken cancellationToken);
        UniTask OnCloseOut(CancellationToken cancellationToken);
    }
}