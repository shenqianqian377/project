# ISO 11820 — Part1 数据层

## 文件清单

```
├── Data/DbHelper.cs        ← SQLite 全部操作 (建表/种子数据/CRUD/迁移)
├── Models/
│   ├── Operator.cs         ← 操作员模型
│   ├── TestMaster.cs       ← 试验主表模型 (40+字段)
│   └── MasterMessage.cs    ← 系统消息模型
└── appsettings.json        ← 全局配置
```

## 数据库 (6张表)

| 表名 | 说明 |
|------|------|
| operators | 操作员账号 |
| apparatus | 设备信息 |
| productmaster | 样品信息 |
| testmaster | 试验记录 (核心) |
| sensors | 传感器配置 (17通道) |
| CalibrationRecords | 校准历史 |

## 对外 API

- `Login(username, pwd, out userid, out usertype)` — 登录验证
- `UpsertProduct(...)` — 写入/更新样品
- `InsertTest(...)` / `UpdateTestResult(...)` — 试验 CRUD
- `QueryTests(from, to, pid)` — 按日期/样品查询
- `GetApparatus(id)` — 查询设备信息
- `InsertCalibration(...)` / `GetCalibrationRecords()` — 校准管理
