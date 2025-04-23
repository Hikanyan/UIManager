# UIManager

```mermaid
flowchart TD
A[Start] --> B[AwakeでInstance登録]
B --> C[Open(prefabKey)]
C --> D[UINodeFactoryでPrefab生成]
D --> E[TransformをCanvasにSetParent]
E --> F[RegisterNode]
F --> G[PushNode]
G --> H[OnInitialize]
H --> I[OnOpenIn]
I --> J[OnOpenOut]
J --> K[UIStackに追加]
K --> L[入力順序修正]
```

```mermaid

classDiagram
    class UIManager {
        - Dictionary<int, IUINode> _activeUiNodes
        - List<IUINode> _uiStack
        - Canvas _rootCanvas
        - bool _inputOrderFixEnabled
        + static UIManager Instance
        + RegisterNode(UINodeBase, CancellationToken)
        + UnregisterNode(int, CancellationToken)
        + Open<T>(string, UINodeBase, CancellationToken) T:UINodeBase
        + Close(int, CancellationToken)
    }

    class IUINode {
        <<interface>>
        + int Id
        + UINodeBase Parent
        + List<IUINode> Children
        + bool IsActive
        + OnInitialize(CancellationToken)
        + OnOpenIn(CancellationToken)
        + OnOpenOut(CancellationToken)
        + OnCloseIn(CancellationToken)
        + OnCloseOut(CancellationToken)
    }

    class UINodeBase {
        + int Id
        + UINodeBase Parent
        + List<IUINode> Children
        + bool IsActive
        + Initialize(id:int, parent:UINodeBase)
        + AddChild(IUINode)
        + RemoveChild(IUINode)
        + SetActiveChild(IUINode)
    }

    UIManager --> IUINode : 操作対象
    IUINode <|.. UINodeBase
```

```mermaid

sequenceDiagram
    participant Client
    participant UIManager
    participant UINodeFactory
    participant UINodeBase

    Client->>UIManager: Open<T>(key, parent, token)
    UIManager->>UINodeFactory: Create<T>()
    UINodeFactory-->>UIManager: instance
    UIManager->>UINodeBase: SetParent(Canvas)
    UIManager->>UIManager: RegisterNode(node, token)
    UIManager->>UINodeBase: OnInitialize(token)
    UIManager->>UINodeBase: OnOpenIn(token)
    UIManager->>UINodeBase: OnOpenOut(token)
    UIManager->>UIManager: Add to _uiStack

    Client->>UIManager: Close(id, token)
    UIManager->>UINodeBase: OnCloseIn(token)
    UIManager->>UINodeBase: OnCloseOut(token)
    UIManager->>UIManager: Remove from _uiStack
    UIManager->>UINodeBase: Destroy(gameObject)
```

