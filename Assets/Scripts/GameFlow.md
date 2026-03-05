# 关卡游戏流程

三个核心角色：

- **GameServer** — 唯一的流程驱动者，监听 `UnitTurnEndedEvent`，插入 UI 等待点，调用 TurnService 推进
- **TurnService** — 纯逻辑，管理回合状态和行动队列，通过事件通知外界
- **InteractionFSM** — 交互层，处理玩家输入，唯一允许调用的 TurnService 方法是 `EndUnitTurn()`


```
启动：
LevelLoader: 初始化一堆东西，然后触发GameServer.StartGame()

GameServer.StartGame()
├── 启动 FSM → 初始状态是 WaitingForSystemState （空状态）
└── 开始一个新的大回合 - StartNewTurn()
    ├── TurnService.StartTurn()
    │       ├── Queue.Build(actionableUnits)
    │       ├── publish TurnOrderChangedEvent(TurnReset) - 触发回合条UI
    │       └── publish TurnStartedEvent - 触发 TurnBanner 显示“第几回合”
    └── 等待 TurnBanner 消失，之后触发 AdvanceToNextUnit() - AwaitThen(AdvanceToNextUnit, Expect: TurnStart)
    
GameServer.AdvanceToNextUnit() - 控制 TurnService 推进
├── TurnService.NextUnit()
│       ├── Queue.Advance(), 跳过死亡/无法行动单位
│       └── publish TurnOrderChangedEvent(UnitAdvanced)
├── publish UnitTurnStartedEvent - 触发 TurnBanner 显示“哪个单位”
└── 等待 TurnBanner 消失，之后触发 StartNewUnitTurn() - AwaitThen(StartNewUnitTurn, Expect: UnitTransition)

GameServer.StartNewUnitTurn() - 要进入玩家操作回合了，判断当前的Unit是什么阵营（目前全都可操作），控制状态机开始干活


下面玩家可以开始操作：（目前默认先进入UnitSelected状态，按右键或者esc可以返回到idle）
UnitSelectedState - 显示行动菜单，等玩家选择
│
├── [Move] → MovementPreviewState - 显示可移动范围和路径预览
│       ├── [点击有效格子] → 创建 MoveUnitCommand → ExecutingState - 等命令执行完
│       │                                           └── DetermineNextState()
│       │                                               ├── 还有AP → 回到 UnitSelectedState 继续操作
│       │                                               └── AP耗尽 → EndUnitTurn() (见"单位回合结束")
│       ├── [Back] → 回到 UnitSelectedState
│       └── [Esc]  → IdleState
│
├── [Wait] → ExecuteWait() - 主动结束这个单位的回合
│       ├── DeselectUnit()
│       └── EndUnitTurn() (见"单位回合结束")
│
├── [点击其他友方单位] → SwitchToUnit() - 切换选中，留在 UnitSelectedState
├── [Back] → IdleState - 取消选中，等玩家重新点击单位
└── [Esc]  → IdleState


单位回合结束：
TurnService.EndUnitTurn() - 由 InteractionFSM 调用（Wait 或 AP 耗尽）
└── publish UnitTurnEndedEvent - GameServer 监听这个事件接管后续
    └── GameServer.OnUnitTurnEnded()
        ├── FSM → WaitingForSystemState - 锁住交互，玩家不能操作了
        ├── 还有单位没行动 → AdvanceToNextUnit() (见"推进到下一个单位")
        └── 全都行动完了 → EndCurrentTurn() (见"大回合结束")
        
        
大回合结束：
GameServer.EndCurrentTurn()
├── TurnService.EndTurn()
│       └── publish TurnEndedEvent - 触发 TurnBanner 显示"回合结束"
└── 等待 TurnBanner 消失，之后触发 StartNewTurn() - AwaitThen(StartNewTurn, Expect: TurnEnd)
    └── 回到"启动流程"的 StartNewTurn()，新的大回合开始
```
