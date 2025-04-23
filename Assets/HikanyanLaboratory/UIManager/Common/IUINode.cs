using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace HikanyanLaboratory.UISystemTest
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

        void OnInitialize(CancellationToken cancellationToken);
        void OnOpenIn(CancellationToken cancellationToken);
        void OnCloseIn(CancellationToken cancellationToken);
        void OnOpenOut(CancellationToken cancellationToken);
        void OnCloseOut(CancellationToken cancellationToken);
    }
}