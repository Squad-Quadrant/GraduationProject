关卡流程目前主要由 GameServer、TurnService与InteractionFSM三者通过事件穿插配合完成

- TurnService — 纯逻辑，管理回合状态和行动队列，不主动推进流程，通过事件通知外界
- GameServer — 唯一的流程驱动者，决定"什么时候做下一步"，监听UnitTurnEnded事件，插入 UI 等待，然后调用 TurnService 推进；
- InteractionFSM — 交互层。处理玩家输入，执行具体的单位操作（移动、攻击等）。 
  - 唯一允许调用的 TurnService 方法是 EndUnitTurn()（大回合Turn的推进会由GameServer在UnitTurnEnded的回调中自己判断）

## 完整流程追踪

### Phase 1: 游戏开始 & 回合启动

```
GameServer.StartGame()
│
▼
GameServer.BeginNewTurn()
│
├── TurnService.StartTurn()
│       ├── TurnNumber++, IsTurnActive = true
│       ├── Queue.Build(actionableUnits)
│       ├── publish TurnOrderChangedEvent(TurnReset)      ←─ UI 可据此初始化行动条
│       └── publish TurnStartedEvent                      ←─ UI 播放"Round N"动画
│
└── AwaitThen(TurnStart, AdvanceToNextUnit)
│
│  ... UI 播放 "Round N begins" 动画 ...
│  ... UI 完成后发布 PresentationCompleteEvent(UI.TurnStart) ...
│
▼
```

### Phase 2: 推进到单位

```
GameServer.AdvanceToNextUnit()
│
├── TurnService.NextUnit()
│       ├── Queue.Advance()  (cursor 前移)
│       ├── skip dead/unable units (while loop)
│       ├── IsUnitActing = true
│       ├── publish TurnOrderChangedEvent(UnitAdvanced)   ←─ UI 更新行动条高亮
│       └── publish UnitTurnStartedEvent                  ←─ UI 播放单位登场/聚焦
│
└── return unit  (控制权转交给 InteractionFSM)
```

### Phase 3: 玩家操作单位 (InteractionFSM)

```
UnitTurnStartedEvent
│
▼
IdleState: 玩家点击当前行动单位
│  CanControlUnit() → 检查 TurnService.ActiveUnit
▼
UnitSelectedState: 显示行动菜单 (Move/Attack/Wait/EndTurn)
│
├── [Move] → MovementPreviewState → ConfirmState → ExecutingState
│                                                       │
│               MoveUnitCommand 执行，等待动画完成         │
│                                                       ▼
│                                               DetermineNextState()
│                                                  ├── CanAct? → UnitSelectedState (继续操作)
│                                                  └── Done?   → EndUnitTurn() → IdleState
│
├── [Wait] → ExecuteWait()
│       ├── TurnService.EndUnitTurn()     ←── 唯一允许的调用
│       └── → IdleState
│
└── [EndTurn] → ExecuteEndTurn()
├── TurnService.EndUnitTurn()     ←── 同上，不调用 EndTurn()
└── → IdleState
```

### Phase 4: 单位回合结束 → GameServer 接管

```
TurnService.EndUnitTurn()
├── IsUnitActing = false
└── publish UnitTurnEndedEvent            ←── GameServer 监听此事件
│
▼
GameServer.OnUnitTurnEnded()
│
└── AwaitThen(UnitTransition, ProcessAfterUnitTurn)
│
│  ... UI 播放过渡动画 (行动条滚动/切换) ...
│  ... UI 完成后发布 PresentationCompleteEvent(UI.UnitTransition) ...
│
▼
GameServer.ProcessAfterUnitTurn()
│
├── if IsTurnComplete → EndCurrentTurn()     (进入 Phase 5)
└── else              → AdvanceToNextUnit()  (回到 Phase 2)
```

### Phase 5: 大回合结束 → 新回合

```
GameServer.EndCurrentTurn()
│
├── TurnService.EndTurn()
│       ├── Queue.Clear(), IsTurnActive = false
│       └── publish TurnEndedEvent            ←─ UI 播放"回合结束"动画
│
└── AwaitThen(TurnEnd, BeginNewTurn)
│
│  ... UI 播放 "Round N complete" 动画 ...
│  ... UI 完成后发布 PresentationCompleteEvent(UI.TurnEnd) ...
│
▼
GameServer.BeginNewTurn()   ←── 回到 Phase 1，循环开始
```
