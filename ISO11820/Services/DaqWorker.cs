namespace ISO11820.Services;

public class DaqWorker
{
    private readonly TestController _controller;
    private readonly SensorSimulator _sim;
    private readonly Dictionary<string, object> _config;
    private bool _enableSimulation;

    public DaqWorker(TestController controller, SensorSimulator sim, Dictionary<string, object> config)
    {
        _controller = controller;
        _sim = sim;
        _config = config;
        _enableSimulation = GetBoolConfig("Simulation:EnableSimulation", true);
    }

    private bool GetBoolConfig(string key, bool defaultValue)
    {
        if (_config.TryGetValue(key, out var val))
        {
            if (val is bool b) return b;
            if (val is string s && bool.TryParse(s, out var parsed)) return parsed;
        }
        return defaultValue;
    }

    public bool EnableSimulation
    {
        get => _enableSimulation;
        set => _enableSimulation = value;
    }

    /// <summary>
    /// 读取所有传感器通道的当前温度值。
    /// 仿真模式从 SensorSimulator 获取；硬件模式从真实串口获取（当前为占位实现）。
    /// </summary>
    public Dictionary<string, double> ReadSensors()
    {
        if (_enableSimulation)
            return new Dictionary<string, double>
            {
                ["TF1"] = _sim.Tf1,
                ["TF2"] = _sim.Tf2,
                ["TS"] = _sim.Ts,
                ["TC"] = _sim.Tc,
                ["TCal"] = _sim.TCal
            };

        // 硬件模式占位：返回固定值（实际应通过 Modbus 读取串口）
        return new Dictionary<string, double>
        {
            ["TF1"] = 25, ["TF2"] = 25, ["TS"] = 25, ["TC"] = 25, ["TCal"] = 25
        };
    }
}