using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace ISO11820.Data;

public class DbHelper
{
    private readonly string _connStr;

    public DbHelper(IConfiguration config)
    {
        string dbPath = Path.Combine(
            config["FileStorage:BaseDirectory"] ?? "D:\\ISO11820",
            config["Database:SqlitePath"] ?? "Data\\ISO11820.db");
        _connStr = $"Data Source={dbPath}";
        EnsureDatabase();
    }

    public DbHelper(string connectionString)
    {
        _connStr = connectionString;
        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();

        // 创建所有表
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS operators (
                userid    TEXT NOT NULL,
                username  TEXT NOT NULL,
                pwd       TEXT NOT NULL,
                usertype  TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS apparatus (
                apparatusid   INTEGER NOT NULL CONSTRAINT PK_apparatus PRIMARY KEY,
                innernumber   TEXT NOT NULL,
                apparatusname TEXT NOT NULL,
                checkdatef    date NOT NULL,
                checkdatet    date NOT NULL,
                pidport       TEXT NOT NULL,
                powerport     TEXT NOT NULL,
                constpower    INTEGER NULL
            );
            CREATE TABLE IF NOT EXISTS productmaster (
                productid   TEXT NOT NULL CONSTRAINT PK_productmaster PRIMARY KEY,
                productname TEXT NOT NULL,
                specific    TEXT NOT NULL,
                diameter    REAL NOT NULL,
                height      REAL NOT NULL,
                flag        TEXT NULL
            );
            CREATE TABLE IF NOT EXISTS testmaster (
                productid        TEXT NOT NULL,
                testid           TEXT NOT NULL,
                testdate         date NOT NULL,
                ambtemp          REAL NOT NULL,
                ambhumi          REAL NOT NULL,
                according        TEXT NOT NULL,
                operator         TEXT NOT NULL,
                apparatusid      TEXT NOT NULL,
                apparatusname    TEXT NOT NULL,
                apparatuschkdate date NOT NULL,
                rptno            TEXT NOT NULL,
                preweight        REAL NOT NULL,
                postweight       REAL NOT NULL,
                lostweight       REAL NOT NULL,
                lostweight_per   REAL NOT NULL,
                totaltesttime    INTEGER NOT NULL,
                constpower       INTEGER NOT NULL,
                phenocode        TEXT NOT NULL,
                flametime        INTEGER NOT NULL,
                flameduration    INTEGER NOT NULL,
                maxtf1           REAL NOT NULL,
                maxtf2           REAL NOT NULL,
                maxts            REAL NOT NULL,
                maxtc            REAL NOT NULL,
                maxtf1_time      INTEGER NOT NULL,
                maxtf2_time      INTEGER NOT NULL,
                maxts_time       INTEGER NOT NULL,
                maxtc_time       INTEGER NOT NULL,
                finaltf1         REAL NOT NULL,
                finaltf2         REAL NOT NULL,
                finalts          REAL NOT NULL,
                finaltc          REAL NOT NULL,
                finaltf1_time    INTEGER NOT NULL,
                finaltf2_time    INTEGER NOT NULL,
                finalts_time     INTEGER NOT NULL,
                finaltc_time     INTEGER NOT NULL,
                deltatf1         REAL NOT NULL,
                deltatf2         REAL NOT NULL,
                deltatf          REAL NOT NULL,
                deltats          REAL NOT NULL,
                deltatc          REAL NOT NULL,
                memo             TEXT NULL,
                flag             TEXT NULL,
                durationmode     INTEGER NULL,
                targetduration   INTEGER NULL,
                CONSTRAINT PK_testmaster PRIMARY KEY (productid, testid)
            );
            CREATE TABLE IF NOT EXISTS sensors (
                sensorid    INTEGER NOT NULL CONSTRAINT PK_sensors PRIMARY KEY,
                sensorname  TEXT NOT NULL,
                dispname    TEXT NOT NULL,
                sensorgroup TEXT NOT NULL,
                unit        TEXT NOT NULL,
                discription TEXT NOT NULL,
                flag        TEXT NOT NULL,
                signalzero  REAL NOT NULL,
                signalspan  REAL NOT NULL,
                outputzero  REAL NOT NULL,
                outputspan  REAL NOT NULL,
                outputvalue REAL NOT NULL,
                inputvalue  REAL NOT NULL,
                signaltype  INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS CalibrationRecords (
                Id                 TEXT NOT NULL CONSTRAINT PK_CalibrationRecords PRIMARY KEY,
                CalibrationDate    TEXT NOT NULL,
                CalibrationType    TEXT NOT NULL,
                ApparatusId        INTEGER NOT NULL,
                Operator           TEXT NOT NULL,
                TemperatureData    TEXT NOT NULL,
                PassedCriteria     INTEGER NOT NULL,
                Remarks            TEXT NOT NULL,
                CreatedAt          TEXT NOT NULL
            );";
        cmd.ExecuteNonQuery();

        // 初始化默认数据
        SeedData(conn);
    }

    private static void SeedData(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();

        // 操作员
        cmd.CommandText = @"
            INSERT OR IGNORE INTO operators (userid, username, pwd, usertype)
            VALUES ('1', 'admin', '123456', 'admin');";
        cmd.ExecuteNonQuery();

        cmd.CommandText = @"
            INSERT OR IGNORE INTO operators (userid, username, pwd, usertype)
            VALUES ('2', 'experimenter', '123456', 'operator');";
        cmd.ExecuteNonQuery();

        // 设备
        cmd.CommandText = @"
            INSERT OR IGNORE INTO apparatus (apparatusid, innernumber, apparatusname,
                checkdatef, checkdatet, pidport, powerport, constpower)
            VALUES (0, 'FURNACE-01', '一号试验炉',
                date('now'), date('now', '+1 year'), 'COM9', 'COM9', 2048);";
        cmd.ExecuteNonQuery();

        // 传感器 (17通道)
        for (int i = 0; i <= 16; i++)
        {
            string dispName = i switch
            {
                0 => "炉温1",
                1 => "炉温2",
                2 => "表面温度",
                3 => "中心温度",
                16 => "校准温度",
                _ => $"备用通道{i + 1}"
            };
            string groupName = i switch
            {
                0 or 1 or 2 or 3 => "采集",
                16 => "校准",
                _ => "备用"
            };

            cmd.CommandText = $@"
                INSERT OR IGNORE INTO sensors
                    (sensorid, sensorname, dispname, sensorgroup, unit, discription,
                     flag, signalzero, signalspan, outputzero, outputspan,
                     outputvalue, inputvalue, signaltype)
                VALUES ({i}, 'Sensor{i}', '{dispName}', '{groupName}', '℃',
                    '{dispName}', '启用', 0, 0, 0, 1000, 0, 0, 4);";
            cmd.ExecuteNonQuery();
        }
    }

    /// <summary>登录验证 (按 username + pwd 校验)</summary>
    public bool Login(string username, string pwd, out string userId, out string userType)
    {
        userId = "";
        userType = "";
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT userid, usertype FROM operators WHERE username = $name AND pwd = $pwd";
        cmd.Parameters.AddWithValue("$name", username);
        cmd.Parameters.AddWithValue("$pwd", pwd);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            userId = reader.GetString(0);
            userType = reader.GetString(1);
            return true;
        }
        return false;
    }

    /// <summary>插入或更新样品信息</summary>
    public void UpsertProduct(string productId, string productName, string spec,
                               double diameter, double height)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO productmaster (productid, productname, specific, diameter, height, flag)
            VALUES ($pid, $name, $spec, $diameter, $height, '')
            ON CONFLICT(productid) DO UPDATE SET
                productname = $name, specific = $spec,
                diameter = $diameter, height = $height;";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$name", productName);
        cmd.Parameters.AddWithValue("$spec", spec);
        cmd.Parameters.AddWithValue("$diameter", diameter);
        cmd.Parameters.AddWithValue("$height", height);
        cmd.ExecuteNonQuery();
    }

    /// <summary>新建试验 (初始插入，统计字段填 0)</summary>
    public void InsertTest(string productId, string testId, string operatorName,
                            double preweight, double ambtemp, double ambhumi,
                            int durationMode, int targetDuration)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO testmaster
                (productid, testid, testdate, operator, ambtemp, ambhumi,
                 according, apparatusid, apparatusname, apparatuschkdate, rptno,
                 preweight, postweight, lostweight, lostweight_per,
                 totaltesttime, constpower, phenocode, flametime, flameduration,
                 maxtf1,maxtf2,maxts,maxtc,
                 maxtf1_time,maxtf2_time,maxts_time,maxtc_time,
                 finaltf1,finaltf2,finalts,finaltc,
                 finaltf1_time,finaltf2_time,finalts_time,finaltc_time,
                 deltatf1,deltatf2,deltatf,deltats,deltatc,
                 durationmode,targetduration)
            VALUES
                ($pid,$tid,date('now'),$op,$ambtemp,$ambhumi,
                 'ISO 11820:2022', 'FURNACE-01', '一号试验炉', date('now'), $rptno,
                 $prewt,0,0,0,
                 0,0,'',0,0,
                 0,0,0,0,0,0,0,0,
                 0,0,0,0,0,0,0,0,
                 0,0,0,0,0,
                 $dmode,$tdur)";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        cmd.Parameters.AddWithValue("$op", operatorName);
        cmd.Parameters.AddWithValue("$ambtemp", ambtemp);
        cmd.Parameters.AddWithValue("$ambhumi", ambhumi);
        cmd.Parameters.AddWithValue("$rptno", productId);
        cmd.Parameters.AddWithValue("$prewt", preweight);
        cmd.Parameters.AddWithValue("$dmode", durationMode);
        cmd.Parameters.AddWithValue("$tdur", targetDuration);
        cmd.ExecuteNonQuery();
    }

    /// <summary>试验完成后更新统计字段</summary>
    public void UpdateTestResult(
        string productId, string testId,
        double postweight, double lostweight, double lostweightPer,
        int totalTestTime, int constPower,
        double maxTf1, double maxTf2, double maxTs, double maxTc,
        int maxTf1Time, int maxTf2Time, int maxTsTime, int maxTcTime,
        double finalTf1, double finalTf2, double finalTs, double finalTc,
        double deltaTf1, double deltaTf2, double deltaTf, double deltaTs, double deltaTc,
        string phenoCode, int flameTime, int flameDuration,
        string memo)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE testmaster SET
                postweight      = $postwt,
                lostweight      = $lostwt,
                lostweight_per  = $lostper,
                totaltesttime   = $totaltime,
                constpower      = $constp,
                phenocode       = $pheno,
                flametime       = $flamet,
                flameduration   = $flamed,
                maxtf1          = $mtf1, maxtf2         = $mtf2,
                maxts           = $mts,  maxtc          = $mtc,
                maxtf1_time     = $mtf1t, maxtf2_time    = $mtf2t,
                maxts_time      = $msts,  maxtc_time     = $mtct,
                finaltf1        = $ftf1, finaltf2        = $ftf2,
                finalts         = $fts,  finaltc         = $ftc,
                deltatf1        = $dtf1, deltatf2        = $dtf2,
                deltatf         = $dtf,  deltats         = $dts,
                deltatc         = $dtc,
                memo            = $memo,
                flag            = '10000000'
            WHERE productid=$pid AND testid=$tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        cmd.Parameters.AddWithValue("$postwt", postweight);
        cmd.Parameters.AddWithValue("$lostwt", lostweight);
        cmd.Parameters.AddWithValue("$lostper", lostweightPer);
        cmd.Parameters.AddWithValue("$totaltime", totalTestTime);
        cmd.Parameters.AddWithValue("$constp", constPower);
        cmd.Parameters.AddWithValue("$pheno", phenoCode);
        cmd.Parameters.AddWithValue("$flamet", flameTime);
        cmd.Parameters.AddWithValue("$flamed", flameDuration);
        cmd.Parameters.AddWithValue("$mtf1", maxTf1);
        cmd.Parameters.AddWithValue("$mtf2", maxTf2);
        cmd.Parameters.AddWithValue("$mts", maxTs);
        cmd.Parameters.AddWithValue("$mtc", maxTc);
        cmd.Parameters.AddWithValue("$mtf1t", maxTf1Time);
        cmd.Parameters.AddWithValue("$mtf2t", maxTf2Time);
        cmd.Parameters.AddWithValue("$msts", maxTsTime);
        cmd.Parameters.AddWithValue("$mtct", maxTcTime);
        cmd.Parameters.AddWithValue("$ftf1", finalTf1);
        cmd.Parameters.AddWithValue("$ftf2", finalTf2);
        cmd.Parameters.AddWithValue("$fts", finalTs);
        cmd.Parameters.AddWithValue("$ftc", finalTc);
        cmd.Parameters.AddWithValue("$dtf1", deltaTf1);
        cmd.Parameters.AddWithValue("$dtf2", deltaTf2);
        cmd.Parameters.AddWithValue("$dtf", deltaTf);
        cmd.Parameters.AddWithValue("$dts", deltaTs);
        cmd.Parameters.AddWithValue("$dtc", deltaTc);
        cmd.Parameters.AddWithValue("$memo", memo);
        cmd.ExecuteNonQuery();
    }

    /// <summary>查询试验历史列表</summary>
    public List<Dictionary<string, object>> QueryTests(
        DateTime from, DateTime to, string productId = "", string operatorName = "")
    {
        var result = new List<Dictionary<string, object>>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM testmaster
            WHERE testdate BETWEEN $from AND $to
              AND ($pid = '' OR productid LIKE '%' || $pid || '%')
              AND ($op = '' OR operator = $op)
            ORDER BY testdate DESC";
        cmd.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$op", operatorName);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            result.Add(row);
        }
        return result;
    }

    /// <summary>获取单个试验详情</summary>
    public Dictionary<string, object>? GetTestById(string productId, string testId)
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM testmaster WHERE productid = $pid AND testid = $tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            return row;
        }
        return null;
    }

    /// <summary>获取所有样品列表</summary>
    public List<Dictionary<string, object>> GetAllProducts()
    {
        var result = new List<Dictionary<string, object>>();
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM productmaster ORDER BY productid";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            result.Add(row);
        }
        return result;
    }

    /// <summary>获取设备信息</summary>
    public Dictionary<string, object>? GetApparatus()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM apparatus LIMIT 1";
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
                row[reader.GetName(i)] = reader.GetValue(i);
            return row;
        }
        return null;
    }
}