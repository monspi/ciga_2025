# 精确碰撞体设置指南

## 概述

新的精确碰撞体功能允许ObjectSetupTool和AutoSetupCollision根据PNG素材的非透明部分自动生成精确的碰撞体，而不是使用简单的矩形或圆形。

## 主要功能

### 1. 透明度检测
- 自动分析PNG图片的Alpha通道
- 根据透明度阈值确定哪些像素是"实体"
- 只为非透明部分生成碰撞体

### 2. 多种碰撞体类型支持
- **Box Collider**: 基于非透明区域的精确边界框
- **Circle Collider**: 基于非透明区域的最小外接圆
- **Capsule Collider**: 基于非透明区域的胶囊形状
- **Polygon Collider**: 完全精确的多边形轮廓

### 3. 性能优化
- 多边形简化算法减少顶点数量
- 可调整的简化容差
- 智能边界计算

## 配置参数

### ObjectSetupTool 新增参数

#### 精确碰撞体设置
- **启用精确碰撞体** (`enablePreciseCollider`): 是否启用基于透明度的精确检测
- **透明度阈值** (`alphaThreshold`): 0-1范围，低于此值的像素视为透明
- **多边形简化容差** (`polygonSimplification`): 值越大形状越简单，性能越好

#### 碰撞体类型
- **Box**: 精确边界框，覆盖所有非透明像素的最小矩形
- **Circle**: 精确外接圆，基于非透明区域的最大尺寸
- **Capsule**: 精确胶囊，基于非透明区域的形状
- **Polygon**: 完全精确的多边形轮廓

### AutoSetupCollision 新增参数

#### 精确碰撞体设置
- **启用精确碰撞体** (`enablePreciseCollider`): 单个对象的精确检测开关
- **透明度阈值** (`alphaThreshold`): 透明度判断阈值

## 使用方法

### 方法一：使用ObjectSetupTool批量设置

1. **创建ObjectSetupTool**
   ```
   - 在场景中创建空对象
   - 添加ObjectSetupTool组件
   ```

2. **配置精确碰撞体参数**
   ```
   - 勾选"启用精确碰撞体"
   - 设置透明度阈值（建议0.1）
   - 选择碰撞体类型：
     * Polygon - 最精确，适合复杂形状
     * Box - 精确边界框，适合矩形物体
     * Circle - 精确圆形，适合圆形物体
   ```

3. **批量应用**
   ```
   - 点击"查找候选对象"
   - 点击"批量设置对象"
   - 工具会为每个对象生成精确碰撞体
   ```

### 方法二：使用AutoSetupCollision单独设置

1. **添加组件**
   ```
   - 选择需要精确碰撞体的对象
   - 添加AutoSetupCollision组件
   ```

2. **配置参数**
   ```
   - 勾选"启用精确碰撞体"
   - 设置透明度阈值
   - 选择碰撞体类型为Polygon
   ```

3. **自动应用**
   ```
   - 组件会在Awake时自动生成精确碰撞体
   - 或手动调用"Setup Components"
   ```

## 技术原理

### 透明度检测算法

1. **像素扫描**
   ```csharp
   // 扫描sprite区域内的每个像素
   for (int y = startY; y < endY; y++)
   {
       for (int x = startX; x < endX; x++)
       {
           Color32 pixel = pixels[y * textureWidth + x];
           if (pixel.a > alphaThreshold * 255)
           {
               // 这是非透明像素
               // 更新边界信息
           }
       }
   }
   ```

2. **边界计算**
   - 找到所有非透明像素的最小包围矩形
   - 转换为Unity世界坐标系
   - 考虑sprite的pivot和pixelsPerUnit

3. **多边形生成**
   - 使用Unity内置的GetPhysicsShape API
   - 应用多边形简化算法
   - 移除冗余顶点以提高性能

### 坐标系转换

```csharp
// 从纹理坐标转换为Unity坐标
float left = (minX - pivot.x) / pixelsPerUnit;
float right = (maxX - pivot.x) / pixelsPerUnit;
float bottom = (minY - pivot.y) / pixelsPerUnit;
float top = (maxY - pivot.y) / pixelsPerUnit;
```

## 使用建议

### 1. 纹理设置要求

**必须设置纹理为可读**:
```
- 选择PNG纹理
- 在Inspector中勾选"Read/Write Enabled"
- 点击Apply应用设置
```

**推荐的纹理导入设置**:
```
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 根据游戏需要调整
- Filter Mode: Point (no filter) 或 Bilinear
- Read/Write Enabled: 勾选（必需）
```

### 2. 性能考虑

**透明度阈值选择**:
- **0.1**: 适合大多数情况，忽略几乎透明的像素
- **0.5**: 更严格，只保留明显不透明的部分
- **0.01**: 保留所有有颜色的像素

**多边形简化**:
- **0-1**: 保持原始精度，适合小型精细对象
- **2-5**: 适中简化，平衡精度和性能
- **5-10**: 大幅简化，适合大型或远景对象

**碰撞体类型选择**:
- **Polygon**: 最精确，但计算成本最高
- **Box**: 快速且适合矩形物体
- **Circle**: 最快，适合圆形物体

### 3. 调试和验证

**检查生成结果**:
```
- 在Scene视图中选择对象
- 查看碰撞体的绿色轮廓
- 确认轮廓准确覆盖非透明部分
```

**常见问题排查**:
1. **轮廓不准确**: 调整透明度阈值
2. **性能问题**: 增加多边形简化容差
3. **无法生成**: 检查纹理是否可读

## 示例配置

### 精细游戏角色
```
- 碰撞体类型: Polygon
- 启用精确碰撞体: true
- 透明度阈值: 0.1
- 多边形简化: 1.0
```

### 地形元素
```
- 碰撞体类型: Polygon
- 启用精确碰撞体: true
- 透明度阈值: 0.2
- 多边形简化: 3.0
```

### 简单道具
```
- 碰撞体类型: Box
- 启用精确碰撞体: true
- 透明度阈值: 0.1
```

### 圆形物体
```
- 碰撞体类型: Circle
- 启用精确碰撞体: true
- 透明度阈值: 0.15
```

## 技术限制

1. **纹理必须可读**: Unity的安全限制要求纹理设置为可读
2. **性能影响**: 复杂多边形会增加物理计算成本
3. **内存使用**: 精确碰撞体比简单形状使用更多内存
4. **编辑器限制**: 某些功能只在编辑器模式下可用

## 故障排除

### 常见错误

1. **"纹理不可读"警告**
   - **解决**: 选择纹理 → Inspector → 勾选"Read/Write Enabled" → Apply

2. **生成的碰撞体不准确**
   - **解决**: 调整透明度阈值，通常0.05-0.2之间

3. **性能问题**
   - **解决**: 增加多边形简化容差到3-5

4. **无法生成多边形**
   - **解决**: 确保sprite有有效的物理形状，或选择其他碰撞体类型

### 调试工具

1. **显示分层信息**: 查看对象的碰撞体配置
2. **Scene视图**: 查看生成的碰撞体轮廓
3. **Console日志**: 查看详细的生成信息和错误

这个功能大大提高了2D游戏中碰撞检测的精确度，特别适合有复杂形状的精灵对象。
