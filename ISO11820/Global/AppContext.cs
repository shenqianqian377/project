using ISO11820.Data;
using ISO11820.Services;
using Microsoft.Extensions.Configuration;

namespace ISO11820.Global;

public static class AppCtx
{
    public static IConfiguration Config { get; private set; } = null!;
    public static DbHelper Db { get; private set; } = null!;
    public static SensorSimulator Simulator { get; private set; } = null!;
    public static TestController Controller { get; private set; } = null!;
    public static DaqWorker Daq { get; private set; } = null!;

    public static string CurrentUserId { get; set; } = "";
    public static string CurrentUserName { get; set; } = "";
    public static string CurrentUserType { get; set; } = "";

    public static void Initialize(IConfiguration config)
    {
        Config = config;

        // 数据库
        string? dbPath = config["Database:SqlitePath"];
        string baseDir = config["FileStorage:BaseDirectory"] ?? "D:\\ISO11820";
        string fullPath = Path.Combine(baseDir, dbPath ?? "Data\\ISO11820.db");
        string connStr = $"Data Source={fullPath}";

        Db = new DbHelper(connStr);

        // 将配置转为字典 (用于兼容 Part2 的 Dictionary<string,object> API)
        var configDict = ConvertConfigToDict(config);

        // 仿真引擎
        Simulator = new SensorSimulator(configDict);

        // 控制器
        Controller = new TestController(Db, Simulator, configDict);

        // 采集服务
        Daq = new DaqWorker(Controller, Simulator, configDict);
    }

    private static Dictionary<string, object> ConvertConfigToDict(IConfiguration config)
    {
        var dict = new Dictionary<string, object>();

        foreach (var section in config.GetChildren())
        {
            foreach (var child in section.GetChildren())
            {
                string key = $"{section.Key}:{child.Key}";

                // 尝试解析为数值类型
                if (bool.TryParse(child.Value, out bool bVal))
                    dict[key] = bVal;
                else if (int.TryParse(child.Value, out int iVal))
                    dict[key] = iVal;
                else if (double.TryParse(child.Value, out double dVal))
                    dict[key] = dVal;
                else if (child.Value != null)
                    dict[key] = child.Value;
            }
        }

        return dict;
    }
}