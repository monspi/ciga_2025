# EnemySetupTool 使用说明

## 概述

EnemySetupTool 是一个强大的敌人批量配置工具，专门用于快速设置和管理场景中的敌人对象。它可以一次性为多个敌人对象配置相同的EnemyConfig，并自动添加必要的组件。

## 主要功能

### 🎯 核心功能
1. **批量标签修改**：自动将对象标签设置为"Enemy"
2. **EnemyController配置**：添加并配置EnemyController组件
3. **统一配置应用**：所有敌人使用相同的EnemyConfig
4. **碰撞系统集成**：自动添加CollisionController组件
5. **2D物理设置**：可选的2D碰撞器和刚体配置
6. **智能查找**：按名称模式或标签查找敌人对象

### 🔧 配置选项
- **敌人配置**：指定要应用的EnemyConfigSO文件
- **批量设置选项**：控制哪些组件需要自动添加
- **物理设置**：配置2D碰撞器类型和刚体属性
- **查找设置**：定义如何查找敌人对象
- **渲染设置**：设置排序层和渲染顺序

## 详细配置说明

### 1. 敌人配置
```csharp
public EnemyConfigSO enemyConfig;  // 要应用到所有敌人的配置数据
```
- **必需设置**：必须指定一个EnemyConfigSO配置文件
- **统一应用**：所有敌人将使用相同的配置参数

### 2. 批量设置选项
```csharp
public bool autoSetEnemyTag = true;           // 自动修改标签为Enemy
public bool autoAddEnemyController = true;    // 自动添加EnemyController组件
public bool autoAddCollisionController = true; // 自动添加碰撞控制组件
public bool autoSetup2DPhysics = true;        // 自动设置2D物理组件
```

### 3. 物理设置
```csharp
public ColliderType colliderType = ColliderType.BoxCollider2D;  // 碰撞器类型
public bool isTrigger = false;                                  // 是否设置为触发器
public bool addRigidbody2D = false;                            // 是否添加刚体
public RigidbodyType2D rigidbodyType = RigidbodyType2D.Kinematic; // 刚体类型
```

**支持的碰撞器类型：**
- BoxCollider2D
- CircleCollider2D
- PolygonCollider2D
- CapsuleCollider2D

### 4. 查找设置
```csharp
public string enemyNamePattern = "Enemy";  // 按名称模式查找
public bool useContainsMatch = true;       // 是否使用包含匹配
public List<GameObject> manualEnemyList;   // 手动指定的敌人列表
```

**查找逻辑：**
- 首先通过"Enemy"标签查找现有敌人
- 然后按名称模式查找候选对象
- 支持精确匹配或包含匹配
- 结果会添加到手动列表中

### 5. 渲染设置
```csharp
public bool autoSetSortingLayer = true;       // 是否自动设置排序层
public string sortingLayerName = "Default";   // 排序层名称
public int baseSortingOrder = 0;              // 基础排序顺序
```

## 使用步骤

### 第一步：创建EnemyConfig配置文件
1. 右键点击Project窗口
2. 选择 `Create > FartGame > Enemy Config`
3. 配置敌人参数（初始耐力、标签、描述等）

### 第二步：设置EnemySetupTool
1. 在场景中创建空对象，命名为"EnemySetupTool"
2. 添加 `EnemySetupTool` 组件
3. 将创建的EnemyConfig拖拽到 `Enemy Config` 字段
4. 根据需要调整各项设置

### 第三步：查找和配置敌人
1. 设置 `Enemy Name Pattern`（如"Enemy"、"Monster"等）
2. 点击 **"查找敌人对象"** 按钮
3. 检查 `Manual Enemy List` 确认找到的对象
4. 点击 **"批量设置敌人"** 按钮完成配置

## Inspector 按钮说明

### 🔍 查找敌人对象
- **功能**：在场景中查找符合条件的敌人候选对象
- **逻辑**：先按标签查找，再按名称模式查找
- **结果**：更新 `manualEnemyList` 列表

### ⚙️ 批量设置敌人
- **功能**：为列表中的所有敌人对象应用配置
- **过程**：
  1. 检查EnemyConfig是否设置
  2. 为每个敌人对象设置标签
  3. 添加和配置EnemyController
  4. 添加CollisionController
  5. 设置2D物理组件（可选）
  6. 设置渲染属性（可选）
  7. 确保CollisionManager存在

### 📊 显示配置状态
- **功能**：显示当前配置状态和每个敌人的组件情况
- **输出**：在Console窗口显示详细状态信息

### 🧹 清理配置
- **功能**：移除所有敌人对象的相关组件
- **操作**：
  - 移除EnemyController组件
  - 移除CollisionController组件
  - 重置标签为"Untagged"
- **警告**：此操作不可撤销

### 🗑️ 清空敌人列表
- **功能**：清空 `manualEnemyList` 列表
- **用途**：重新开始查找和配置过程

## 工作流程示例

### 标准工作流程
```
1. 创建EnemyConfig配置文件
   ↓
2. 在场景中添加EnemySetupTool
   ↓
3. 设置EnemyConfig和查找参数
   ↓
4. 查找敌人对象
   ↓
5. 批量设置敌人
   ↓
6. 验证配置状态
```

### 调试工作流程
```
1. 显示配置状态 → 检查当前状态
   ↓
2. 清理配置 → 重置所有设置
   ↓
3. 重新配置 → 使用新的参数
   ↓
4. 验证结果 → 确认配置正确
```

## 最佳实践

### 1. 配置准备
- 提前创建好EnemyConfig配置文件
- 确保场景中的敌人对象有合适的名称
- 为不同类型的敌人使用不同的命名模式

### 2. 分批处理
- 如果有多种类型的敌人，分别创建不同的EnemyConfig
- 可以使用多个EnemySetupTool实例处理不同类型
- 使用不同的名称模式区分敌人类型

### 3. 验证和测试
- 配置完成后使用"显示配置状态"验证结果
- 测试敌人的行为是否符合预期
- 检查碰撞系统是否正常工作

### 4. 版本控制
- 在大量修改前备份场景
- 可以使用"清理配置"功能恢复原始状态
- 保存不同版本的EnemyConfig以便对比

## 故障排除

### 常见问题

1. **找不到敌人对象**
   - 检查名称模式是否正确
   - 确认对象是否在场景中激活
   - 尝试调整 `useContainsMatch` 设置

2. **配置失败**
   - 确认EnemyConfig是否已设置
   - 检查Console窗口的错误信息
   - 验证对象是否有必要的组件

3. **物理组件问题**
   - 检查是否已启用2D物理设置
   - 确认碰撞器类型是否适合对象
   - 验证层级设置是否正确

4. **性能问题**
   - 对于大量敌人，考虑分批处理
   - 禁用不需要的自动设置选项
   - 优化EnemyConfig中的参数

### 调试技巧

1. **使用Console输出**：
   ```csharp
   // 工具会输出详细的配置过程
   Debug.Log($"成功配置敌人: {enemyObj.name}");
   ```

2. **检查组件状态**：
   ```csharp
   // 使用"显示配置状态"查看每个敌人的组件情况
   bool hasController = enemy.GetComponent<EnemyController>() != null;
   ```

3. **分步骤配置**：
   - 先只启用标签设置
   - 然后添加控制器
   - 最后添加物理组件

## 扩展功能

### 自定义碰撞器类型
可以修改 `ColliderType` 枚举添加新的碰撞器类型：

```csharp
public enum ColliderType
{
    BoxCollider2D,
    CircleCollider2D,
    PolygonCollider2D,
    CapsuleCollider2D,
    EdgeCollider2D  // 新增类型
}
```

### 添加自定义配置
可以扩展工具以支持更多的敌人属性配置：

```csharp
[Header("高级设置")]
public bool autoSetNavigation = false;  // 自动设置导航
public bool autoSetAnimation = false;   // 自动设置动画
```

### 批量操作优化
对于大量敌人，可以添加进度条显示：

```csharp
for (int i = 0; i < manualEnemyList.Count; i++)
{
    EditorUtility.DisplayProgressBar("配置敌人", $"处理 {i+1}/{manualEnemyList.Count}", 
        (float)i / manualEnemyList.Count);
    // 配置逻辑...
}
EditorUtility.ClearProgressBar();
```

## 总结

EnemySetupTool 提供了一个完整的敌人配置解决方案，大大简化了敌人对象的设置过程。通过统一的配置文件和智能的批量操作，可以快速为整个游戏场景配置一致的敌人行为和属性。

主要优势：
- **效率高**：批量处理，节省时间
- **一致性**：统一配置，减少错误
- **灵活性**：多种查找方式，适应不同场景
- **可扩展**：易于添加新功能和配置选项
