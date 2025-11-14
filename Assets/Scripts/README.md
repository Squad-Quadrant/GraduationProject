- Core: 核心框架，纯C#，引擎无关
    - Events: 事件系统
    - FSM: 状态机
    - Utilities: 通用工具类

- Data: 数据层
    - Config: 配置数据
    - Runtime: 跨系统的运行时数据
      - Events: 事件定义

- Systems: 游戏规则，逻辑层，通过依赖注入提供接口给Unity层调用
    - Turn: 回合系统
    - Combat: 战斗系统
    - Map: 地图系统

- Presentation: 引擎相关--表现层
    - Bootstrap: 依赖注入器，游戏初始化，游戏的Main函数，可以分批次注入依赖
        - RootContainer.cs: 全局依赖注入容器
        - LevelContainer.cs: 关卡依赖注入容器
    - Input
    - UI
