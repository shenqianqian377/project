namespace ISO11820.Services;

public class SensorSimulator
{
    private readonly Random _rng = new();
    private readonly Dictionary<string, object> _config;

    // 5 通道温度
    public double Tf1 { get; private set; }
    public double Tf2 { get; private set; }
    public double Ts { get; private set; }
    public double Tc { get; private set; }
    public double TCal { get; private set; }

    // 配置参数
    public double TargetTemp { get; }
    public double HeatingRate { get; }
    public double Fluctuation { get; }
    public double StableThreshold { get; }

    // 工作模式
    public bool IsHeating { get; set; }
    public bool IsConstantPower { get; set; }
    public bool IsCooling { get; set; }

    // ──────────────── 常数值 ────────────────
    private const double TickSeconds = 0.8;          // 每 tick 秒数
    private const double CoolRate = 0.5;             // 冷却速率 °C/tick
    private const double SurfaceNonRecordRatio = 0.3; // 非记录阶段 TS/TF1 比例
    private const double CenterNonRecordRatio = 0.25;
    private const double SurfaceRecordMaxRatio = 0.95; // 记录阶段 TS 目标上限比例
    private const double CenterRecordMaxRatio = 0.85;
    private const double SurfaceApproachRate = 0.02;   // 记录阶段 TS 指数趋近速率
    private const double CenterApproachRate = 0.01;
    private const double SurfaceMaxTemp = 800;
    private const double CenterMaxTemp = 750;
    private const double CalNoiseMultiplier = 2.0;
    private const double CoolNoiseMultiplier = 0.1;
    private const double MinTemp = 25;

    public SensorSimulator(Dictionary<string, object> config)
    {
        _config = config;
        double initTemp = GetDoubleConfig("Simulation:InitialFurnaceTemp", 25.0);
        Tf1 = initTemp;
        Tf2 = initTemp;
        TargetTemp = GetDoubleConfig("Simulation:TargetFurnaceTemp", 750.0);
        HeatingRate = GetDoubleConfig("Simulation:HeatingRatePerSecond", 5.0);
        Fluctuation = GetDoubleConfig("Simulation:TempFluctuation", 0.5);
        StableThreshold = GetDoubleConfig("Simulation:StableThreshold", 3.0);
        Ts = initTemp * SurfaceNonRecordRatio;
        Tc = initTemp * CenterNonRecordRatio;
        TCal = initTemp;
    }

    private double GetDoubleConfig(string key, double defaultValue)
    {
        if (_config.TryGetValue(key, out var val))
        {
            if (val is double d) return d;
            if (val is long l) return l;
            if (val is int i) return i;
            if (val is string s && double.TryParse(s, out var parsed)) return parsed;
        }
        return defaultValue;
    }

    /// <summary>生成 ±Fluctuation 范围内的随机噪声</summary>
    private double Noise() => (_rng.NextDouble() * 2 - 1) * Fluctuation;

    public void Reset(double initTemp)
    {
        Tf1 = initTemp;
        Tf2 = initTemp;
        Ts = initTemp * SurfaceNonRecordRatio;
        Tc = initTemp * CenterNonRecordRatio;
        TCal = initTemp;
        IsHeating = false;
        IsConstantPower = false;
        IsCooling = false;
    }

    /// <summary>每 800ms 调用一次，更新所有通道温度</summary>
    public void Update(bool isRecording)
    {
        // 1. 炉温核心 (TF1, TF2)
        if (IsCooling)
        {
            Tf1 -= CoolRate + Noise() * CoolNoiseMultiplier;
            if (Tf1 < MinTemp) { Tf1 = MinTemp; IsCooling = false; }
            Tf2 -= CoolRate + Noise() * CoolNoiseMultiplier;
            if (Tf2 < MinTemp) Tf2 = MinTemp;
        }
        else if (IsHeating || IsConstantPower)
        {
            if (Tf1 < TargetTemp - StableThreshold)
            {
                // 升温阶段
                Tf1 += HeatingRate * TickSeconds + Noise();
                Tf2 += HeatingRate * TickSeconds + Noise();
            }
            else
            {
                // 稳定阶段 — 钳位到目标温度
                Tf1 = TargetTemp + Noise();
                Tf2 = TargetTemp + Noise();
            }
        }

        // 2. 表面/中心温度 (TS, TC)
        if (isRecording)
        {
            // 记录阶段：指数趋近，模拟热传导
            double surfaceTarget = Math.Min(Tf1 * SurfaceRecordMaxRatio, SurfaceMaxTemp);
            Ts += (surfaceTarget - Ts) * SurfaceApproachRate + Noise();

            double centerTarget = Math.Min(Tf1 * CenterRecordMaxRatio, CenterMaxTemp);
            Tc += (centerTarget - Tc) * CenterApproachRate + Noise();
        }
        else
        {
            // 非记录阶段：近似跟随炉温（低比例）
            Ts = Tf1 * SurfaceNonRecordRatio + Noise();
            Tc = Tf1 * CenterNonRecordRatio + Noise();
        }

        // 3. 校准温度
        TCal = Tf1 + Noise() * CalNoiseMultiplier;

        // 4. 边界处理与精度
        Tf1 = Math.Max(0, Math.Round(Tf1, 1));
        Tf2 = Math.Max(0, Math.Round(Tf2, 1));
        Ts = Math.Max(0, Math.Round(Ts, 1));
        Tc = Math.Max(0, Math.Round(Tc, 1));
        TCal = Math.Max(0, Math.Round(TCal, 1));
    }
}