# Part 4 — 子窗体 (sub-forms)

## 包含文件

| 文件 | 职责 |
|------|------|
| Forms/LoginForm.cs | 登录窗 (角色选择 + 密码验证) |
| Forms/NewTestForm.cs | 新建试验窗 (样品信息 + 环境参数 + 设备自动填入) |
| Forms/TestRecordForm.cs | 试验记录窗 (火焰现象 + 试验后质量 + 触发导出) |

## 各窗体出口数据

```csharp
// LoginForm → 无返回值，登录成功直接打开 MainForm
AppCtx.Instance.CurrentUserId/CurrentUserName/CurrentUserType/OperatorRole

// NewTestForm → 返回以下属性供 Part5 调用 CreateTest
string ProductId, TestId, SampleName, Spec
double Diameter, SampleHeight, PreWeight, AmbTemp, AmbHumi
int    DurationMode, TargetDuration

// TestRecordForm → 内部调用 UpdateTestResult + ExportService
// 参数: TestController (获取温度缓冲、试验数据)
```

## 依赖关系

- 依赖 Part1 (DbHelper: Login, GetApparatus)
- 依赖 Part2 (TestController, SensorDataPoint)
- 依赖 Part3 (ExportService — 仅 TestRecordForm)
- 被 Part5 打开 (ShowDialog)
