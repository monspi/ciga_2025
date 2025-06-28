# ObjectSetupTool 和分层系统使用指南

## 概述

ObjectSetupTool 是用于批量配置图片素材的工具，支持：
- 自动添加 Collider、Rigidbody 和 EnemyController 组件
- 手动分层设置以避免渲染遮挡
- 基于分层的碰撞机制
- 批量处理和清理功能

## 主要组件

### 1. ObjectSetupTool
**位置**: `Assets/Engine/Tool/ObjectSetupTool.cs`
**功能**: 通用对象批量设置工具

**主要功能**:
- 查找候选对象（有SpriteRenderer的对象）
- 批量添加物理组件（Collider2D、Rigidbody2D）
- 批量添加游戏逻辑组件（EnemyController、CollisionController）
- 手动分层管理
- 批量清理和重置

### 2. EnemySetupTool
**位置**: `Assets/Engine/Tool/EnemySetupTool.cs`
**功能**: 专门用于敌人对象的批量设置

**主要功能**:
- 专门针对敌人的快速设置
- 统一应用 EnemyConfigSO 配置
- 自动分层和标签设置
- 敌人专用的清理功能

### 3. CollisionController
**位置**: `Assets/Engine/Controller/CollisionController.cs`
**功能**: 单个对象的碰撞和渲染层级控制

**新增功能**:
- 手动分层优先级设置
- 自动渲染层级计算
- 支持玩家、敌人、普通对象分类

### 4. AutoSetupCollision
**位置**: `Assets/Engine/Controller/AutoSetupCollision.cs`
**功能**: 自动为单个对象添加必要组件

## 使用流程

### 方法一：使用 ObjectSetupTool（推荐用于复杂场景）

1. **创建设置工具**
   ```
   - 在场景中创建空对象
   - 添加 ObjectSetupTool 组件
   ```

2. **配置工具参数**
   ```
   - 设置 EnemyConfigSO 配置文件
   - 选择要添加的组件类型
   - 配置分层设置
   ```

3. **查找候选对象**
   ```
   点击 "查找候选对象" 按钮或在Inspector中调用
   工具会自动找到所有有SpriteRenderer的对象
   ```

4. **批量设置**
   ```
   点击 "批量设置对象" 按钮
   工具会自动为所有对象添加必要组件并设置分层
   ```

### 方法二：使用 EnemySetupTool（推荐用于敌人设置）

1. **创建敌人设置工具**
   ```
   - 在场景中创建空对象
   - 添加 EnemySetupTool 组件
   ```

2. **设置敌人配置**
   ```
   - 设置 EnemyConfigSO 配置文件
   - 配置物理和分层参数
   ```

3. **查找和设置敌人**
   ```
   - 点击 "查找敌人对象"
   - 点击 "批量设置敌人"
   ```

### 方法三：使用 AutoSetupCollision（用于单个对象）

1. **添加组件**
   ```
   - 选择需要设置的对象
   - 添加 AutoSetupCollision 组件
   ```

2. **配置参数**
   ```
   - 设置碰撞体类型
   - 设置物理属性
   - 设置对象分类和分层
   ```

3. **自动设置**
   ```
   组件会在 Awake 时自动执行设置
   或手动调用 "Setup Components"
   ```

## 分层系统详解

### 分层原理

分层系统基于以下优先级计算最终的 `sortingOrder`：

```
最终 sortingOrder = 基础层级 + Y位置层级 + 分层偏移 + 手动偏移

其中：
- 基础层级：SpriteRenderer的初始sortingOrder
- Y位置层级：-Y坐标 * sortingOrderScale（Y值小的在上方）
- 分层偏移：layerPriority * 1000（手动分层）
- 手动偏移：manualSortingOrderOffset
```

### 分层配置

1. **自动分层**
   - 基于Y坐标：Y值越小，显示越靠上
   - 缩放倍数：默认100倍，可调整

2. **手动分层**
   - 分层优先级：数值越大越在上层
   - 分层间隔：建议使用1000以上的间隔
   - 每个对象可以独立设置分层优先级

3. **分层管理**
   - 使用ObjectSetupTool的"重新排列分层"功能
   - 使用EnemySetupTool的"重新设置分层"功能
   - 手动调整CollisionController的layerPriority属性

## 配置参数说明

### ObjectSetupTool 参数

**批量设置选项**:
- `targetObjects`: 要处理的对象列表
- `enemyConfig`: 敌人配置数据

**组件设置**:
- `addCollider`: 添加Collider2D
- `colliderType`: 碰撞体类型（Box/Circle/Capsule）
- `isTrigger`: 设置为Trigger
- `addRigidbody`: 添加Rigidbody2D
- `bodyType`: 刚体类型
- `freezeRotation`: 冻结旋转

**分层设置**:
- `enableManualLayering`: 启用手动分层
- `layerInterval`: 分层间隔
- `baseLayerPriority`: 基础分层优先级

### CollisionController 参数

**对象类型**:
- `isPlayer`: 标记为玩家
- `isEnemy`: 标记为敌人

**渲染设置**:
- `spriteRenderer`: 要控制的渲染器
- `manualSortingOrderOffset`: 手动层级偏移
- `sortingOrderScale`: 层级缩放倍数

**分层设置**:
- `layerPriority`: 手动分层优先级

## 碰撞机制

### 碰撞检测规则

1. **阻挡机制**
   - 玩家无法进入敌人的最小距离范围内
   - 支持全方向阻挡（上下左右）
   - 已击败的敌人不参与碰撞检测

2. **位置修正**
   - 基于移动方向的智能修正
   - 避免突兀的位置跳跃
   - 支持分轴移动（水平/垂直分别处理）

3. **渲染层级**
   - 实时更新基于位置的层级
   - 手动分层优先级覆盖自动层级
   - 支持复杂的多层级场景

## 常用操作

### 设置新场景

1. 使用ObjectSetupTool查找所有图片对象
2. 设置EnemyConfigSO配置
3. 批量设置所有对象
4. 根据需要调整分层优先级
5. 确保CollisionManager存在

### 调整分层

1. 修改ObjectSetupTool的分层参数
2. 使用"重新排列分层"功能
3. 或手动调整CollisionController的layerPriority

### 添加新敌人

1. 使用EnemySetupTool的"查找敌人对象"
2. 将新对象添加到候选列表
3. 使用"批量设置敌人"

### 清理和重置

1. 使用对应工具的"清理"功能
2. 或手动移除组件
3. 重新设置对象

## 故障排除

### 常见问题

1. **分层不正确**
   - 检查layerPriority设置
   - 确认sortingOrderScale参数
   - 使用"显示分层信息"查看当前状态

2. **碰撞不生效**
   - 确认CollisionManager存在
   - 检查对象的isEnemy/isPlayer标记
   - 验证Collider2D设置

3. **性能问题**
   - 减少实时更新频率
   - 使用更大的分层间隔
   - 考虑禁用不必要的对象更新

### 调试工具

1. **显示状态信息**：查看工具当前状态
2. **显示分层信息**：查看所有对象的分层
3. **CollisionManager.RefreshGameObjects()**：重新扫描对象

## 最佳实践

1. **分层规划**
   - 为不同类型的对象分配不同的分层范围
   - 使用足够大的分层间隔（建议1000+）
   - 预留分层空间用于特殊效果

2. **性能优化**
   - 只为需要的对象启用实时分层更新
   - 对于静态对象，可以禁用Update中的层级更新
   - 使用对象池管理大量动态对象

3. **组织结构**
   - 将相关对象组织在父对象下
   - 使用明确的命名规范
   - 为不同类型的对象设置不同的Layer

这个系统提供了完整的对象设置和分层管理功能，能够满足复杂2D游戏的需求。
