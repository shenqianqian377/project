using ISO11820.Data;
using ISO11820.Models;
using MathNet.Numerics;
using System.Timers;

namespace ISO11820.Services;

public enum TestState
{
    Idle = 0,
    Preparing = 1,
    Ready = 2,
    Recording = 3,
    Complete = 4
}

public class DataBroadcastEventArgs : EventArgs
{
    public double Tf1 { get; set; }
    public double Tf2 { get; set; }
    public double Ts { get; set; }
    public double Tc { get; set; }
    public double TCal { get; set; }
    public TestState State { get; set; }
    public int ElapsedSeconds { get; set; }
    public int TotalDuration { get; set; }
    public double TemperatureDrift { get; set; }
    public bool IsStable { get; set; }
    public string ProductId { get; set; } = "";
    public List<MasterMessage> Messages { get; set; } = new();
    public List<double> RecentTf1 { get; set; } = new();
    public List<double> RecentTf2 { get; set; } = new();
    public List<double> RecentTs { get; set; } = new();
    public List<double> RecentTc { get; set; } = new();
    public List<double> TimePoints { get; set; } = new();
}

public class SensorDataPoint
{
    public int ElapsedSeconds { get; set; }
    public double Tf1 { get; set; }
    public double Tf2 { get; set; }
    public double Ts { get; set; }
    public double Tc { get; set; }
    public double TCal { get; set; }
}

public class TestController
{
    private readonly DbHelper _db;
    private readonly SensorSimulator _sim;
    private readonly Dictionary<string, object> _config;
    private readonly System.Timers.Timer _timer;
    private readonly List<double> _pidOutputQueue = new();
    private readonly List<double> _tf1History = new();
    private readonly List<double> _tf2History = new();
    private readonly List<double> _tsHistory = new();
    private readonly List<double> _tcHistory = new();
    private readonly List<double> _timeHistory = new();
    private readonly List<MasterMessage> _pendingMessages = new();
    private readonly List<SensorDataPoint> _sensorDataBuffer = new();
    private int _simTickCount;

    public TestState CurrentState { get; private set; } = TestState.Idle;
    public int ElapsedSeconds { get; private set; }
    public int StableCounter { get; private set; }
    public bool IsStable { get; private set; }
    public double TemperatureDrift { get; private set; }
    public string CurrentProductId { get; private set; } = "";
    public string CurrentTestId { get; private set; } = "";
    public string CurrentOperator { get; set; } = "";
    public int TotalDuration { get; private set; } = 3600;
    public int DurationMode { get; private set; }
    public double PreWeight { get; set; }
    public double AmbTemp { get; set; } = 25;
    public double AmbHumi { get; set; } = 50;
    public double MaxTf1 { get; private set; }
    public double MaxTf2 { get; private set; }
    public double MaxTs { get; private set; }
    public double MaxTc { get; private set; }
    public int MaxTf1Time { get; private set; }
    public int MaxTf2Time { get; private set; }
    public int MaxTsTime { get; private set; }
    public int MaxTcTime { get; private set; }
    public double FinalTf1 { get; private set; }
    public double FinalTf2 { get; private set; }
    public double FinalTs { get; private set; }
    public double FinalTc { get; private set; }
    public bool HasUnsavedResult { get; set; }
    public double ConstPowerValue { get; set; }
    public DateTime TestStartTime { get; set; }

    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;

    // ──────────────── 常数值 ────────────────
    private const int TimerIntervalMs = 800;
    private const double TickToSeconds = 0.8;
    private const int HistoryMaxPoints = 750;           // 750 tick = 600s = 10min
    private const int DriftWindowPoints = 750;          // 温漂计算窗口：10分钟数据
    private const int PidQueueMax = 600;
    private const int StableThresholdTicks = 3;         // 稳定计数器阈值
    private const int DefaultDuration = 3600;
    private const int EarlyTerminationMin = 1800;       // 30 分钟
    private const int TerminationCheckInterval = 300;   // 5 分钟
    private const double DriftThreshold = 2.0;          // °C/10min
    private const int MinValidRecordSeconds = 5;

    public TestController(DbHelper db, SensorSimulator sim, Dictionary<string, object> config)
    {
        _db = db;
        _sim = sim;
        _config = config;
        _timer = new System.Timers.Timer(TimerIntervalMs);
        _timer.Elapsed += OnTimerTick;
    }

    private double GetDoubleConfig(string key, double defaultValue)
    {
        if (_config.TryGetValue(key, out var val))
        {
            if (val is double d) return d;
            if (val is long l) return l;
            if (val is int i) return i;
        }
        return defaultValue;
    }

    // ──────────────── 试验生命周期 ────────────────

    public void CreateTest(string productId, string testId, string operatorName,
                           double preweight, double ambtemp, double ambhumi,
                           int durationMode, int targetDuration,
                           string productName, string spec, double diameter, double height)
    {
        CurrentProductId = productId;
        CurrentTestId = testId;
        CurrentOperator = operatorName;
        PreWeight = preweight;
        AmbTemp = ambtemp;
        AmbHumi = ambhumi;
        DurationMode = durationMode;
        TotalDuration = durationMode == 0 ? DefaultDuration : targetDuration;
        HasUnsavedResult = false;
        ElapsedSeconds = 0;
        MaxTf1 = MaxTf2 = MaxTs = MaxTc = 0;
        MaxTf1Time = MaxTf2Time = MaxTsTime = MaxTcTime = 0;
        FinalTf1 = FinalTf2 = FinalTs = FinalTc = 0;
        _pidOutputQueue.Clear();
        _tf1History.Clear();
        _tf2History.Clear();
        _tsHistory.Clear();
        _tcHistory.Clear();
        _timeHistory.Clear();
        _sensorDataBuffer.Clear();
        _pendingMessages.Clear();
        _simTickCount = 0;
        StableCounter = 0;
        IsStable = false;
        ConstPowerValue = GetDoubleConfig("Hardware:ConstPower", 2048);

        double initTemp = GetDoubleConfig("Simulation:InitialFurnaceTemp", 25.0);
        _sim.Reset(initTemp);
        CurrentState = TestState.Idle;

        _db.UpsertProduct(productId, productName, spec, diameter, height);
        _db.InsertTest(productId, testId, operatorName, preweight, ambtemp, ambhumi, durationMode, TotalDuration);

        AddMessage("新试验已创建，样品编号：" + productId);
        Broadcast();
    }

    public void StartHeating()
    {
        if (CurrentState != TestState.Idle) return;
        _sim.IsHeating = true;
        _sim.IsCooling = false;
        _sim.IsConstantPower = false;
        CurrentState = TestState.Preparing;
        _timer.Start();
        HasUnsavedResult = false;
        AddMessage("开始升温，系统升温中");
        Broadcast();
    }

    public void StopHeating()
    {
        if (CurrentState != TestState.Preparing && CurrentState != TestState.Ready && CurrentState != TestState.Complete) return;
        _sim.IsHeating = false;
        _sim.IsConstantPower = false;
        _sim.IsCooling = true;
        CurrentState = TestState.Idle;
        AddMessage("停止升温，系统冷却中");
        Broadcast();
    }

    public void StartRecording()
    {
        if (CurrentState != TestState.Ready) return;

        if (!_pidOutputQueue.Any())
            ConstPowerValue = GetDoubleConfig("Hardware:ConstPower", 2048);
        else
            ConstPowerValue = _pidOutputQueue.Average();

        _sim.IsConstantPower = true;
        _sim.IsHeating = true;
        CurrentState = TestState.Recording;
        ElapsedSeconds = 0;
        TestStartTime = DateTime.Now;
        AddMessage("开始记录，计时开始");
        Broadcast();
    }

    public void StopRecording()
    {
        if (CurrentState != TestState.Recording) return;

        _sim.IsConstantPower = false;

        if (ElapsedSeconds >= MinValidRecordSeconds)
        {
            // 记录最终值
            FinalTf1 = _sim.Tf1;
            FinalTf2 = _sim.Tf2;
            FinalTs = _sim.Ts;
            FinalTc = _sim.Tc;

            CurrentState = TestState.Complete;
            HasUnsavedResult = true;
            AddMessage("用户手动停止记录");
        }
        else
        {
            CurrentState = TestState.Preparing;
            ElapsedSeconds = 0;
            AddMessage("记录时间过短，返回升温状态");
        }

        Broadcast();
    }

    /// <summary>
    /// 保存试验结果到数据库，并生成 CSV / Excel / PDF 文件
    /// </summary>
    public void SaveTestResult(double postWeight, string phenoCode,
                                int flameTime, int flameDuration, string memo)
    {
        if (!HasUnsavedResult) return;

        double lostWeight = PreWeight - postWeight;
        double lostWeightPer = PreWeight > 0 ? Math.Round(lostWeight / PreWeight * 100, 2) : 0;

        double deltaTf1 = Math.Round(FinalTf1 - AmbTemp, 1);
        double deltaTf2 = Math.Round(FinalTf2 - AmbTemp, 1);
        double deltaTs = Math.Round(FinalTs - AmbTemp, 1);
        double deltaTc = Math.Round(FinalTc - AmbTemp, 1);
        double deltaTf = deltaTs; // 综合温升取表面温升

        // 更新数据库
        _db.UpdateTestResult(
            CurrentProductId, CurrentTestId,
            postWeight, lostWeight, lostWeightPer,
            ElapsedSeconds, (int)ConstPowerValue,
            MaxTf1, MaxTf2, MaxTs, MaxTc,
            MaxTf1Time, MaxTf2Time, MaxTsTime, MaxTcTime,
            FinalTf1, FinalTf2, FinalTs, FinalTc,
            deltaTf1, deltaTf2, deltaTf, deltaTs, deltaTc,
            phenoCode, flameTime, flameDuration, memo);

        // 写入 CSV
        WriteCsvFile();

        // 标记已保存
        HasUnsavedResult = false;
        AddMessage("试验结果已保存");
        Broadcast();
    }

    private void WriteCsvFile()
    {
        try
        {
            string baseDir = GetStringConfig("FileStorage:TestDataDirectory", "D:\\ISO11820\\TestData");
            string dir = Path.Combine(baseDir, CurrentProductId, CurrentTestId);
            Directory.CreateDirectory(dir);
            string filePath = Path.Combine(dir, "sensor_data.csv");

            using var writer = new StreamWriter(filePath, false);
            writer.WriteLine("Time,Temp1,Temp2,TempSurface,TempCenter,TempCalibration");
            foreach (var point in _sensorDataBuffer)
            {
                writer.WriteLine(
                    $"{point.ElapsedSeconds},{point.Tf1},{point.Tf2},{point.Ts},{point.Tc},{point.TCal}");
            }
        }
        catch (Exception ex)
        {
            AddMessage($"CSV 写入失败：{ex.Message}", true);
        }
    }

    private string GetStringConfig(string key, string defaultValue)
    {
        if (_config.TryGetValue(key, out var val) && val is string s)
            return s;
        return defaultValue;
    }

    public void MarkSaved()
    {
        HasUnsavedResult = false;
        _sim.IsHeating = true;
        _sim.IsCooling = false;
        _sim.IsConstantPower = true;
        CurrentState = TestState.Preparing;
        StableCounter = 0;
        IsStable = false;
        _timer.Start();
    }

    public void LoadExistingTest(Dictionary<string, object> testData)
    {
        CurrentProductId = testData["productid"].ToString()!;
        CurrentTestId = testData["testid"].ToString()!;
        CurrentOperator = testData["operator"].ToString()!;
        PreWeight = Convert.ToDouble(testData["preweight"]);
        AmbTemp = Convert.ToDouble(testData["ambtemp"]);
        AmbHumi = Convert.ToDouble(testData["ambhumi"]);
        DurationMode = Convert.ToInt32(testData["durationmode"]);
        TotalDuration = Convert.ToInt32(testData["targetduration"]);
        HasUnsavedResult = testData["flag"].ToString() != "10000000";

        double initTemp = GetDoubleConfig("Simulation:InitialFurnaceTemp", 25.0);
        _sim.Reset(initTemp);
        CurrentState = TestState.Idle;
    }

    // ──────────────── 定时器逻辑 ────────────────

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        if (CurrentState == TestState.Idle && !_sim.IsCooling) return;
        if (CurrentState == TestState.Complete) return;

        bool isRecording = CurrentState == TestState.Recording;
        _sim.Update(isRecording);

        if (CurrentState == TestState.Recording)
        {
            ElapsedSeconds++;
            _sensorDataBuffer.Add(new SensorDataPoint
            {
                ElapsedSeconds = ElapsedSeconds,
                Tf1 = _sim.Tf1,
                Tf2 = _sim.Tf2,
                Ts = _sim.Ts,
                Tc = _sim.Tc,
                TCal = _sim.TCal
            });

            _pidOutputQueue.Add(_sim.Tf1);
            if (_pidOutputQueue.Count > PidQueueMax) _pidOutputQueue.RemoveAt(0);

            if (_sim.Tf1 > MaxTf1) { MaxTf1 = _sim.Tf1; MaxTf1Time = ElapsedSeconds; }
            if (_sim.Tf2 > MaxTf2) { MaxTf2 = _sim.Tf2; MaxTf2Time = ElapsedSeconds; }
            if (_sim.Ts > MaxTs) { MaxTs = _sim.Ts; MaxTsTime = ElapsedSeconds; }
            if (_sim.Tc > MaxTc) { MaxTc = _sim.Tc; MaxTcTime = ElapsedSeconds; }

            CheckTermination();
        }

        if (CurrentState == TestState.Preparing)
        {
            if (_sim.Tf1 >= 745 && _sim.Tf1 <= 755)
            {
                StableCounter++;
                if (StableCounter > StableThresholdTicks)
                {
                    IsStable = true;
                    CurrentState = TestState.Ready;
                    _sim.IsConstantPower = true;
                    AddMessage("温度已稳定，可以开始记录");
                }
            }
            else
            {
                StableCounter = 0;
                IsStable = false;
            }
        }

        if (CurrentState == TestState.Ready)
        {
            _pidOutputQueue.Add(_sim.Tf1);
            if (_pidOutputQueue.Count > PidQueueMax) _pidOutputQueue.RemoveAt(0);

            if (_sim.Tf1 < 745 || _sim.Tf1 > 755)
            {
                StableCounter = 0;
                IsStable = false;
                CurrentState = TestState.Preparing;
                AddMessage("温度波动，回到升温状态");
            }
        }

        _simTickCount++;
        _tf1History.Add(_sim.Tf1);
        _tf2History.Add(_sim.Tf2);
        _tsHistory.Add(_sim.Ts);
        _tcHistory.Add(_sim.Tc);
        _timeHistory.Add(_simTickCount * TickToSeconds);

        // 保留最近 10 分钟数据 (750 tick × 0.8s = 600s)
        while (_tf1History.Count > HistoryMaxPoints)
        {
            _tf1History.RemoveAt(0); _tf2History.RemoveAt(0);
            _tsHistory.RemoveAt(0); _tcHistory.RemoveAt(0);
            _timeHistory.RemoveAt(0);
        }

        CalculateTemperatureDrift();
        Broadcast();
    }

    private void CheckTermination()
    {
        if (DurationMode == 0)
        {
            // 标准 60 分钟模式
            if (ElapsedSeconds >= DefaultDuration)
            {
                CurrentState = TestState.Complete;
                HasUnsavedResult = true;
                _sim.IsConstantPower = false;
                AddMessage($"记录时间到达 {DefaultDuration} 秒，试验自动结束");
                return;
            }

            // 检查提前终止：≥30min 且每 5min 检查
            if (ElapsedSeconds >= EarlyTerminationMin && ElapsedSeconds % TerminationCheckInterval == 0)
            {
                if (IsStable && Math.Abs(TemperatureDrift) < DriftThreshold)
                {
                    CurrentState = TestState.Complete;
                    HasUnsavedResult = true;
                    _sim.IsConstantPower = false;
                    AddMessage("满足终止条件，试验结束", true);
                }
            }
        }
        else
        {
            // 自定义时长模式
            if (ElapsedSeconds >= TotalDuration)
            {
                CurrentState = TestState.Complete;
                HasUnsavedResult = true;
                _sim.IsConstantPower = false;
                AddMessage($"记录时间到达 {TotalDuration} 秒，试验自动结束");
            }
        }
    }

    private void CalculateTemperatureDrift()
    {
        // 需要至少 60 个数据点 (≈48秒) 才开始计算
        if (_tf1History.Count < 60) { TemperatureDrift = 0; return; }

        // 取最近 DriftWindowPoints 个点做线性回归
        var recentTf1 = _tf1History.Skip(Math.Max(0, _tf1History.Count - DriftWindowPoints)).ToList();
        if (recentTf1.Count < 2) { TemperatureDrift = 0; return; }

        try
        {
            double[] x = Enumerable.Range(0, recentTf1.Count).Select(i => (double)i).ToArray();
            double[] y = recentTf1.ToArray();
            (double intercept, double slope) = Fit.Line(x, y);
            // 将斜率从 每tick °C 转为 °C/10min
            // 每个点间隔 0.8s，600秒 = 750个点
            TemperatureDrift = Math.Round(slope * 600.0 / TickToSeconds, 2);
        }
        catch
        {
            TemperatureDrift = 0;
        }
    }

    // ──────────────── 消息 & 广播 ────────────────

    private void AddMessage(string msg, bool isWarning = false)
    {
        _pendingMessages.Add(new MasterMessage
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Message = msg,
            IsWarning = isWarning
        });
    }

    private void Broadcast()
    {
        DataBroadcast?.Invoke(this, new DataBroadcastEventArgs
        {
            Tf1 = _sim.Tf1,
            Tf2 = _sim.Tf2,
            Ts = _sim.Ts,
            Tc = _sim.Tc,
            TCal = _sim.TCal,
            State = CurrentState,
            ElapsedSeconds = ElapsedSeconds,
            TotalDuration = TotalDuration,
            TemperatureDrift = TemperatureDrift,
            IsStable = IsStable,
            ProductId = CurrentProductId,
            Messages = new List<MasterMessage>(_pendingMessages),
            RecentTf1 = new List<double>(_tf1History),
            RecentTf2 = new List<double>(_tf2History),
            RecentTs = new List<double>(_tsHistory),
            RecentTc = new List<double>(_tcHistory),
            TimePoints = new List<double>(_timeHistory)
        });
        _pendingMessages.Clear();
    }

    // ──────────────── 辅助方法 ────────────────

    public static string GetStateText(TestState state) => state switch
    {
        TestState.Idle => "空闲",
        TestState.Preparing => "升温中",
        TestState.Ready => "就绪",
        TestState.Recording => "记录中",
        TestState.Complete => "完成",
        _ => "未知"
    };

    public List<SensorDataPoint> GetSensorDataBuffer()
    {
        return new List<SensorDataPoint>(_sensorDataBuffer);
    }

    public Dictionary<string, object> GetCurrentData()
    {
        return new Dictionary<string, object>
        {
            ["Tf1"] = _sim.Tf1,
            ["Tf2"] = _sim.Tf2,
            ["Ts"] = _sim.Ts,
            ["Tc"] = _sim.Tc,
            ["TCal"] = _sim.TCal,
            ["State"] = CurrentState,
            ["ElapsedSeconds"] = ElapsedSeconds,
            ["TemperatureDrift"] = TemperatureDrift,
            ["IsStable"] = IsStable,
            ["AmbTemp"] = AmbTemp,
            ["AmbHumi"] = AmbHumi
        };
    }
}