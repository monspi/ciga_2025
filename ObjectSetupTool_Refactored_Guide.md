# ObjectSetupTool 重构完成 - 通用对象配置工具

## 概述

`ObjectSetupTool` 已完全重构为智能对象配置工具，可以自动识别和配置三种类型的对象：
- **Player**: 玩家角色
- **Background**: 背景/障碍物
- **Enemy**: 敌人（支持战斗交互）

## 核心功能

### 1. 智能对象识别
工具会根据对象名称中的关键词自动识别对象类型：

#### Player 关键词
- "player", "主角", "角色", "hero", "character"

#### Background 关键词  
- "background", "bg", "地图", "scene", "wall", "obstacle", "建筑", "terrain"

#### Enemy 关键词
- "enemy", "敌人", "monster", "boss", "npc", "mob"

### 2. 类型特定配置

#### Player 对象配置
- **标签**: Player
- **碰撞体**: 默认Box，非Trigger（实体碰撞）
- **刚体**: Dynamic（支持物理交互）
- **组件**: PlayerController、CollisionController
- **行为**: 正常移动，受碰撞阻挡

#### Background 对象配置
- **标签**: Background
- **碰撞体**: 默认Box，非Trigger（实体碰撞）
- **刚体**: Static（静态障碍物）
- **组件**: CollisionController
- **行为**: 玩家无法穿模，但无法交互

#### Enemy 对象配置
- **标签**: Enemy
- **碰撞体**: 默认Box，Trigger（允许重叠）
- **刚体**: Kinematic（可控制移动）
- **组件**: EnemyController、BattleInteraction、CollisionController
- **行为**: 玩家靠近时按空格键进入战斗

## 使用方法

### 1. 基础使用步骤

1. **在场景中创建空对象并添加 ObjectSetupTool 组件**
2. **配置工具参数**（可选，使用默认设置即可）
3. **点击 "查找候选对象"** 自动搜索需要配置的对象
4. **检查识别结果**，确认对象类型正确
5. **点击 "批量设置对象"** 自动配置所有对象

### 2. 高级配置选项

#### 强制类型设定
- 设置 `forceObjectType` 覆盖自动识别
- 可强制所有对象为同一类型

#### 碰撞体精度
- 启用 `enablePreciseCollider` 使用基于PNG透明度的精确碰撞体
- 调整 `alphaThreshold` 控制透明度阈值

#### 分层控制
- 启用 `enableManualLayering` 自动设置显示层级
- 配置 `layerInterval` 控制层级间隔

## 战斗交互系统

### BattleInteraction 组件
自动为Enemy对象添加的战斗交互组件，功能包括：

#### 交互检测
- 自动检测玩家与敌人的距离
- 默认交互距离: 2 单位
- 默认交互键: 空格键

#### UI提示
- 玩家接近时显示交互提示
- 离开时自动隐藏提示
- 可自定义提示文本

#### 战斗逻辑
- 按空格键触发战斗
- 保存当前游戏状态（玩家位置、敌人信息）
- 加载战斗场景

#### 可视化
- 在Scene视图中显示交互范围（黄色圆圈）

## 配置参数详解

### 对象识别设置
```csharp
[Header("对象类型识别")]
public List<string> playerKeywords;    // 玩家关键词
public List<string> backgroundKeywords; // 背景关键词  
public List<string> enemyKeywords;     // 敌人关键词
```

### 类型特定设置
```csharp
[Header("Player专用设置")]
public ColliderType playerColliderType = ColliderType.Box;
public bool playerIsTrigger = false;
public bool addPlayerController = true;

[Header("Background专用设置")]
public ColliderType backgroundColliderType = ColliderType.Box;
public bool backgroundIsTrigger = false;
public RigidbodyType2D backgroundBodyType = RigidbodyType2D.Static;

[Header("Enemy专用设置")]
public ColliderType enemyColliderType = ColliderType.Box;
public bool enemyIsTrigger = true;
public RigidbodyType2D enemyBodyType = RigidbodyType2D.Kinematic;
public bool addEnemyController = true;
public bool addBattleInteraction = true;
```

## 右键菜单操作

### ObjectSetupTool 组件菜单
- **查找候选对象**: 自动搜索场景中的对象
- **批量设置对象**: 配置所有找到的对象
- **清理所有配置**: 移除已配置的组件
- **设置选中对象**: 仅配置当前选中的对象

## 使用场景示例

### 场景1: 初始化游戏场景
```
1. 场景包含: player_character, background_wall, enemy_goblin
2. 运行ObjectSetupTool后:
   - player_character -> Player类型，实体碰撞，可移动
   - background_wall -> Background类型，静态阻挡
   - enemy_goblin -> Enemy类型，支持战斗交互
```

### 场景2: 大批量敌人配置
```
1. 设置 forceObjectType = Enemy
2. 查找所有需要配置的对象
3. 批量设置为敌人，统一添加战斗交互
```

### 场景3: 地图障碍物配置
```
1. 设置 forceObjectType = Background
2. 所有对象配置为静态障碍物
3. 玩家无法穿越，但不会触发特殊交互
```

## 注意事项

### 对象命名规范
- 建议使用有意义的命名，包含类型关键词
- 例如: "player_main", "wall_brick", "enemy_dragon"

### 碰撞体设置
- Player: 非Trigger，支持实体碰撞和阻挡
- Background: 非Trigger，阻挡移动但无特殊交互
- Enemy: Trigger，允许重叠以便检测交互

### 性能考虑
- 大量对象建议分批处理
- 启用 `excludeConfiguredObjects` 避免重复配置
- 精确碰撞体会增加计算开销

## 扩展功能

### 自定义交互键
```csharp
BattleInteraction interaction = enemy.GetComponent<BattleInteraction>();
interaction.interactionKey = KeyCode.E; // 改为E键交互
```

### 自定义交互距离
```csharp
interaction.interactionDistance = 3f; // 3单位交互距离
```

### 自定义战斗场景
```csharp
interaction.battleSceneName = "CustomBattleScene";
```

## 调试功能

### 详细日志
启用 `showDetailedLogs` 查看详细配置过程：
- 对象识别结果
- 组件添加过程
- 配置统计信息

### 可视化调试
- Scene视图显示交互范围
- Console输出配置统计
- 实时显示对象类型识别结果

这个重构的ObjectSetupTool现在是一个真正通用的对象配置工具，能够智能识别不同类型的对象并应用适当的配置，大大简化了游戏开发过程！
