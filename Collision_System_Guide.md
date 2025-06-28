# 碰撞控制系统使用说明

## 概述

新的碰撞控制系统实现了以下功能：
1. **渲染层级控制**：Y值越小的对象显示在越上方
2. **移动限制**：主角无法越过敌人的Y值位置
3. **自动化管理**：通过CollisionManager统一管理所有碰撞逻辑

## 主要组件

### 1. CollisionManager
- **位置**：`Assets/Engine/System/CollisionManager.cs`
- **功能**：全局单例管理器，统一处理所有碰撞相关逻辑
- **自动功能**：
  - 持续更新所有对象的渲染层级
  - 检查和限制玩家移动
  - 管理敌人状态变化

### 2. CollisionController
- **位置**：`Assets/Engine/Controller/CollisionController.cs`
- **功能**：单个对象的碰撞控制组件
- **设置**：
  - `isPlayer`：标记为玩家对象
  - `isEnemy`：标记为敌人对象
  - `spriteRenderer`：指定要控制层级的渲染器

### 3. AutoSetupCollision
- **位置**：`Assets/Engine/Controller/AutoSetupCollision.cs`
- **功能**：自动为对象添加必要的碰撞组件
- **使用**：在Awake时自动执行设置

### 4. EnemySetupTool
- **位置**：`Assets/Engine/Tool/EnemySetupTool.cs`
- **功能**：敌人批量设置工具，专门用于配置敌人对象
- **特性**：
  - 批量修改对象标签为"Enemy"
  - 自动添加和配置EnemyController组件
  - 统一应用EnemyConfig配置
  - 自动设置2D物理组件和碰撞控制

## 快速设置指南

### 方法一：使用敌人设置工具（推荐）

1. 在场景中创建一个空对象
2. 添加 `EnemySetupTool` 组件
3. 在Inspector中设置 `EnemyConfig` 配置文件
4. 点击"查找敌人对象"按钮查找候选对象
5. 点击"批量设置敌人"按钮完成配置
6. 工具会自动：
   - 修改对象标签为"Enemy"
   - 添加EnemyController组件并应用配置
   - 添加CollisionController组件
   - 设置2D物理组件（可选）
   - 创建CollisionManager（如果不存在）

### 方法二：手动设置

1. **创建CollisionManager**：
   ```csharp
   GameObject managerObj = new GameObject("CollisionManager");
   managerObj.AddComponent<CollisionManager>();
   ```

2. **设置玩家对象**：
   ```csharp
   // 添加到玩家GameObject
   CollisionController playerCollision = player.AddComponent<CollisionController>();
   playerCollision.isPlayer = true;
   playerCollision.spriteRenderer = player.GetComponent<SpriteRenderer>();
   ```

3. **设置敌人对象**：
   ```csharp
   // 添加到敌人GameObject
   CollisionController enemyCollision = enemy.AddComponent<CollisionController>();
   enemyCollision.isEnemy = true;
   enemyCollision.spriteRenderer = enemy.GetComponent<SpriteRenderer>();
   ```

## 系统工作原理

### 渲染层级控制
- 每帧自动计算：`sortingOrder = -Y坐标 * 100`
- Y值小的对象会有更大的sortingOrder，显示在上方

### 移动限制逻辑
- CollisionManager持续检查玩家位置
- 当玩家距离敌人太近且试图移动到敌人上方时：
  - 自动将玩家位置修正到敌人下方
  - 更新PlayerModel中的位置数据

### 敌人状态管理
- 当敌人被击败时，自动从碰撞检测中移除
- 支持敌人重生和状态重置

## 配置参数

### CollisionManager 参数
- `minPlayerEnemyDistance`：玩家与敌人的最小距离（默认0.8f）
- `sortingOrderScale`：渲染层级缩放倍数（默认100f）

### CollisionController 参数
- `sortingOrderScale`：单个对象的层级缩放倍数（默认100f）

## 注意事项

1. **标签设置**：确保玩家对象有"Player"标签，敌人对象有"Enemy"标签
2. **SpriteRenderer**：所有需要层级控制的对象必须有SpriteRenderer组件
3. **单例模式**：CollisionManager使用单例模式，场景中只能有一个实例
4. **性能考虑**：系统会在每帧更新层级，对于大量对象可能需要优化

## 故障排除

### 常见问题

1. **渲染层级不正确**
   - 检查SpriteRenderer是否正确设置
   - 确认sortingOrderScale参数
   - 验证CollisionController组件是否正确添加

2. **移动限制不生效**
   - 确认CollisionManager存在且为单例
   - 检查敌人对象是否正确标记为`isEnemy = true`
   - 验证PlayerController是否正确集成了CollisionManager

3. **性能问题**
   - 减少sortingOrderScale的更新频率
   - 考虑只在位置变化时更新层级
   - 对于静态对象，可以禁用持续更新

## 扩展功能

### 添加新的碰撞类型
继承CollisionController或修改其逻辑来支持新的对象类型：

```csharp
public bool isNPC = false;  // 新增NPC类型
public bool isPickup = false;  // 新增拾取物类型
```

### 自定义移动限制规则
在CollisionManager中修改`GetCorrectedPlayerPosition`方法来实现自定义的移动限制逻辑。

### 优化渲染层级更新
可以通过事件系统只在对象位置变化时更新层级，而不是每帧更新。

## 集成说明

此系统已经与现有的PlayerController和EnemyController集成：

- **PlayerController**：自动使用CollisionManager进行移动限制
- **EnemyController**：在被击败时通知CollisionManager
- **FartSystem**：与碰撞系统兼容，不需要额外修改

系统设计为向后兼容，现有代码无需大幅修改即可使用新功能。
