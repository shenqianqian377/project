using Microsoft.Data.Sqlite;

namespace ISO11820.Data;

public class DbHelper
{
    private readonly string _connStr;

    public DbHelper(string dbPath)
    {
        _connStr = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    public SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection(_connStr);
        conn.Open();
        return conn;
    }

    private void InitializeDatabase()
    {
        var dir = Path.GetDirectoryName(_connStr.Replace("Data Source=", ""));
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var conn = new SqliteConnection(_connStr);
        conn.Open();
        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS operators (
                userid   TEXT NOT NULL,
                username TEXT NOT NULL,
                pwd      TEXT NOT NULL,
                usertype TEXT NOT NULL
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
                durationmode     INTEGER NOT NULL DEFAULT 0,
                targetduration   INTEGER NOT NULL DEFAULT 3600,
                CONSTRAINT PK_testmaster PRIMARY KEY (productid, testid),
                CONSTRAINT FK_testmaster_productmaster FOREIGN KEY (productid) REFERENCES productmaster (productid)
            );

            CREATE INDEX IF NOT EXISTS IX_Testmaster_Testdate ON testmaster (testdate);
            CREATE INDEX IF NOT EXISTS IX_Testmaster_Operator ON testmaster (operator);
            CREATE INDEX IF NOT EXISTS IX_Testmaster_Testdate_Productid ON testmaster (testdate, productid);

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
                UniformityResult   REAL NULL,
                MaxDeviation       REAL NULL,
                AverageTemperature REAL NULL,
                PassedCriteria     INTEGER NOT NULL,
                Remarks            TEXT NOT NULL,
                CreatedAt          TEXT NOT NULL,
                TempA1 REAL NULL, TempA2 REAL NULL, TempA3 REAL NULL,
                TempB1 REAL NULL, TempB2 REAL NULL, TempB3 REAL NULL,
                TempC1 REAL NULL, TempC2 REAL NULL, TempC3 REAL NULL,
                TAvg REAL NULL,
                TAvgAxis1 REAL NULL, TAvgAxis2 REAL NULL, TAvgAxis3 REAL NULL,
                TAvgLevela REAL NULL, TAvgLevelb REAL NULL, TAvgLevelc REAL NULL,
                TDevAxis1 REAL NULL, TDevAxis2 REAL NULL, TDevAxis3 REAL NULL,
                TDevLevela REAL NULL, TDevLevelb REAL NULL, TDevLevelc REAL NULL,
                TAvgDevAxis REAL NULL, TAvgDevLevel REAL NULL,
                CenterTempData TEXT NULL,
                Memo TEXT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_CalibrationRecord_Date ON CalibrationRecords (CalibrationDate);
            CREATE INDEX IF NOT EXISTS IX_CalibrationRecord_Operator ON CalibrationRecords (Operator);
        ";
        cmd.ExecuteNonQuery();

        MigrateDatabase(conn);
        SeedDefaultData(conn);
    }

    private void MigrateDatabase(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        try { cmd.CommandText = "ALTER TABLE testmaster ADD COLUMN memo TEXT NULL"; cmd.ExecuteNonQuery(); } catch { }
        try { cmd.CommandText = "ALTER TABLE testmaster ADD COLUMN durationmode INTEGER NOT NULL DEFAULT 0"; cmd.ExecuteNonQuery(); } catch { }
        try { cmd.CommandText = "ALTER TABLE testmaster ADD COLUMN targetduration INTEGER NOT NULL DEFAULT 3600"; cmd.ExecuteNonQuery(); } catch { }
    }

    private void SeedDefaultData(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM operators";
        var count = (long)cmd.ExecuteScalar()!;
        if (count > 0) return;

        cmd.CommandText = @"
            INSERT INTO operators (userid, username, pwd, usertype)
            SELECT '1', 'admin', '123456', 'admin'
            WHERE NOT EXISTS (SELECT 1 FROM operators WHERE username = 'admin');

            INSERT INTO operators (userid, username, pwd, usertype)
            SELECT '2', 'experimenter', '123456', 'operator'
            WHERE NOT EXISTS (SELECT 1 FROM operators WHERE username = 'experimenter');

            INSERT INTO apparatus (apparatusid, innernumber, apparatusname, checkdatef, checkdatet, pidport, powerport, constpower)
            SELECT 0, 'FURNACE-01', '一号试验炉', date('now'), date('now','+1 year'), 'COM9', 'COM9', 2048
            WHERE NOT EXISTS (SELECT 1 FROM apparatus);

            INSERT INTO sensors (sensorid, sensorname, dispname, sensorgroup, unit, discription, flag, signalzero, signalspan, outputzero, outputspan, outputvalue, inputvalue, signaltype) VALUES
            (0, 'Sensor0', '炉温1', '采集', '℃', '炉温1', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (1, 'Sensor1', '炉温2', '采集', '℃', '炉温2', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (2, 'Sensor2', '表面温度', '采集', '℃', '表面温度', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (3, 'Sensor3', '中心温度', '采集', '℃', '中心温度', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (4, 'Sensor4', '备用通道5', '备用', '℃', '备用通道5', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (5, 'Sensor5', '备用通道6', '备用', '℃', '备用通道6', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (6, 'Sensor6', '备用通道7', '备用', '℃', '备用通道7', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (7, 'Sensor7', '备用通道8', '备用', '℃', '备用通道8', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (8, 'Sensor8', '备用通道9', '备用', '℃', '备用通道9', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (9, 'Sensor9', '备用通道10', '备用', '℃', '备用通道10', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (10, 'Sensor10', '备用通道11', '备用', '℃', '备用通道11', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (11, 'Sensor11', '备用通道12', '备用', '℃', '备用通道12', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (12, 'Sensor12', '备用通道13', '备用', '℃', '备用通道13', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (13, 'Sensor13', '备用通道14', '备用', '℃', '备用通道14', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (14, 'Sensor14', '备用通道15', '备用', '℃', '备用通道15', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (15, 'Sensor15', '备用通道16', '备用', '℃', '备用通道16', '启用', 0, 0, 0, 1000, 0, 0, 4),
            (16, 'Sensor16', '校准温度', '校准', '℃', '校准温度', '启用', 0, 0, 0, 1000, 0, 0, 4);
        ";
        cmd.ExecuteNonQuery();
    }

    public bool Login(string username, string pwd, out string userid, out string usertype)
    {
        userid = "";
        usertype = "";
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT userid, usertype FROM operators WHERE username=$name AND pwd=$pwd";
        cmd.Parameters.AddWithValue("$name", username);
        cmd.Parameters.AddWithValue("$pwd", pwd);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            userid = reader.GetString(0);
            usertype = reader.GetString(1);
            return true;
        }
        return false;
    }

    public Dictionary<string, object> GetApparatus(int apparatusId)
    {
        var result = new Dictionary<string, object>();
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM apparatus WHERE apparatusid=$id";
        cmd.Parameters.AddWithValue("$id", apparatusId);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            result["apparatusid"] = reader.GetInt32(0);
            result["innernumber"] = reader.GetString(1);
            result["apparatusname"] = reader.GetString(2);
            result["checkdatef"] = reader.GetString(3);
            result["checkdatet"] = reader.GetString(4);
            result["constpower"] = reader.IsDBNull(7) ? (object)2048 : reader.GetInt32(7);
        }
        return result;
    }

    public void UpsertProduct(string productId, string productName, string spec, double diameter, double height)
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO productmaster (productid, productname, specific, diameter, height, flag)
            VALUES ($pid, $pname, $spec, $diam, $h, NULL)
            ON CONFLICT(productid) DO UPDATE SET
                productname=excluded.productname, specific=excluded.specific,
                diameter=excluded.diameter, height=excluded.height";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$pname", productName);
        cmd.Parameters.AddWithValue("$spec", spec);
        cmd.Parameters.AddWithValue("$diam", diameter);
        cmd.Parameters.AddWithValue("$h", height);
        cmd.ExecuteNonQuery();
    }

    public bool TestExists(string productId, string testId)
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM testmaster WHERE productid=$pid AND testid=$tid";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        return (long)cmd.ExecuteScalar()! > 0;
    }

    public void InsertTest(string productId, string testId, string operatorName,
                           double preweight, double ambtemp, double ambhumi,
                           int durationMode, int targetDuration)
    {
        using var conn = CreateConnection();
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
                 memo, flag, durationmode, targetduration)
            VALUES
                ($pid,$tid,date('now'),$op,$ambt,$ambh,
                 'ISO 11820:2022','FURNACE-01','一号试验炉',date('now'),$rptno,
                 $pre,0,0,0,
                 0,0,'',0,0,
                 0,0,0,0,0,0,0,0,
                 0,0,0,0,0,0,0,0,
                 0,0,0,0,0,
                 NULL, '', $dmode, $tdur)";
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        cmd.Parameters.AddWithValue("$op", operatorName);
        cmd.Parameters.AddWithValue("$ambt", ambtemp);
        cmd.Parameters.AddWithValue("$ambh", ambhumi);
        cmd.Parameters.AddWithValue("$rptno", productId);
        cmd.Parameters.AddWithValue("$pre", preweight);
        cmd.Parameters.AddWithValue("$dmode", durationMode);
        cmd.Parameters.AddWithValue("$tdur", targetDuration);
        cmd.ExecuteNonQuery();
    }

    public void UpdateTestResult(string productId, string testId, double preweight,
                                 double postweight, double lostweight, double lostPer,
                                 double deltaTf1, double deltaTf2, double deltaTf, double deltaTs, double deltaTc,
                                 int totalTime, string phenocode,
                                 double maxTf1, double maxTf2, double maxTs, double maxTc,
                                 int maxTf1Time, int maxTf2Time, int maxTsTime, int maxTcTime,
                                 double finalTf1, double finalTf2, double finalTs, double finalTc,
                                 int finalTf1Time, int finalTf2Time, int finalTsTime, int finalTcTime,
                                 double constPower, int flameTime, int flameDuration)
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE testmaster SET
                postweight=$post, lostweight=$lost, lostweight_per=$lostper,
                deltatf1=$dtf1, deltatf2=$dtf2, deltatf=$dtf, deltats=$dts, deltatc=$dtc,
                totaltesttime=$tt, phenocode=$pheno, flag='10000000',
                flametime=$ftime, flameduration=$fdur,
                maxtf1=$mtf1, maxtf2=$mtf2, maxts=$mts, maxtc=$mtc,
                maxtf1_time=$mtf1t, maxtf2_time=$mtf2t, maxts_time=$mtst, maxtc_time=$mtct,
                finaltf1=$ftf1, finaltf2=$ftf2, finalts=$fts, finaltc=$ftc,
                finaltf1_time=$ftf1t, finaltf2_time=$ftf2t, finalts_time=$ftst, finaltc_time=$ftct,
                constpower=$cp
            WHERE productid=$pid AND testid=$tid";
        cmd.Parameters.AddWithValue("$post", postweight);
        cmd.Parameters.AddWithValue("$lost", lostweight);
        cmd.Parameters.AddWithValue("$lostper", lostPer);
        cmd.Parameters.AddWithValue("$dtf1", deltaTf1);
        cmd.Parameters.AddWithValue("$dtf2", deltaTf2);
        cmd.Parameters.AddWithValue("$dtf", deltaTf);
        cmd.Parameters.AddWithValue("$dts", deltaTs);
        cmd.Parameters.AddWithValue("$dtc", deltaTc);
        cmd.Parameters.AddWithValue("$tt", totalTime);
        cmd.Parameters.AddWithValue("$pheno", phenocode);
        cmd.Parameters.AddWithValue("$ftime", flameTime);
        cmd.Parameters.AddWithValue("$fdur", flameDuration);
        cmd.Parameters.AddWithValue("$mtf1", maxTf1);
        cmd.Parameters.AddWithValue("$mtf2", maxTf2);
        cmd.Parameters.AddWithValue("$mts", maxTs);
        cmd.Parameters.AddWithValue("$mtc", maxTc);
        cmd.Parameters.AddWithValue("$mtf1t", maxTf1Time);
        cmd.Parameters.AddWithValue("$mtf2t", maxTf2Time);
        cmd.Parameters.AddWithValue("$mtst", maxTsTime);
        cmd.Parameters.AddWithValue("$mtct", maxTcTime);
        cmd.Parameters.AddWithValue("$ftf1", finalTf1);
        cmd.Parameters.AddWithValue("$ftf2", finalTf2);
        cmd.Parameters.AddWithValue("$fts", finalTs);
        cmd.Parameters.AddWithValue("$ftc", finalTc);
        cmd.Parameters.AddWithValue("$ftf1t", finalTf1Time);
        cmd.Parameters.AddWithValue("$ftf2t", finalTf2Time);
        cmd.Parameters.AddWithValue("$ftst", finalTsTime);
        cmd.Parameters.AddWithValue("$ftct", finalTcTime);
        cmd.Parameters.AddWithValue("$cp", constPower);
        cmd.Parameters.AddWithValue("$pid", productId);
        cmd.Parameters.AddWithValue("$tid", testId);
        cmd.ExecuteNonQuery();
    }

    public List<Dictionary<string, object>> QueryTests(DateTime from, DateTime to, string productId)
    {
        var result = new List<Dictionary<string, object>>();
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT productid, testid, testdate, operator, preweight, postweight,
                   lostweight_per, deltatf, totaltesttime, phenocode, flag, ambtemp, ambhumi
            FROM testmaster
            WHERE testdate BETWEEN $from AND $to
              AND ($pid = '' OR productid LIKE '%' || $pid || '%')
            ORDER BY testdate DESC";
        cmd.Parameters.AddWithValue("$from", from.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$to", to.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("$pid", productId);
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

    public Dictionary<string, object>? GetTestDetail(string productId, string testId)
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM testmaster WHERE productid=$pid AND testid=$tid";
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

    public List<Dictionary<string, object>> GetSensorDataFromCsv(string productId, string testId)
    {
        var result = new List<Dictionary<string, object>>();
        var baseDir = GetConfigValue("D:\\ISO11820");
        var filePath = Path.Combine(baseDir, "TestData", productId, testId, "sensor_data.csv");
        if (!File.Exists(filePath)) return result;

        var lines = File.ReadAllLines(filePath);
        if (lines.Length <= 1) return result;

        var headers = lines[0].Split(',');
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            var row = new Dictionary<string, object>();
            row["elapsed_seconds"] = int.Parse(values[0]);
            row["tf1"] = double.Parse(values[1]);
            row["tf2"] = double.Parse(values[2]);
            row["ts"] = double.Parse(values[3]);
            row["tc"] = double.Parse(values[4]);
            row["tcal"] = double.Parse(values[5]);
            result.Add(row);
        }
        return result;
    }

    public Dictionary<string, object>? GetLatestTest()
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT * FROM testmaster WHERE flag != '10000000' ORDER BY testdate DESC LIMIT 1";
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

    public void InsertCalibration(string calDate, string operatorName, string tempPoints, string notes)
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        var id = Guid.NewGuid().ToString();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
        cmd.CommandText = @"INSERT INTO CalibrationRecords
            (Id, CalibrationDate, CalibrationType, ApparatusId, Operator,
             TemperatureData, PassedCriteria, Remarks, CreatedAt)
            VALUES ($id, $caldate, 'Surface', 0, $op, $td, 1, $rem, $cat)";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.Parameters.AddWithValue("$caldate", calDate);
        cmd.Parameters.AddWithValue("$op", operatorName);
        cmd.Parameters.AddWithValue("$td", tempPoints);
        cmd.Parameters.AddWithValue("$rem", notes);
        cmd.Parameters.AddWithValue("$cat", now);
        cmd.ExecuteNonQuery();
    }

    public List<Dictionary<string, object>> GetCalibrationRecords()
    {
        var result = new List<Dictionary<string, object>>();
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, CalibrationDate, Operator, TemperatureData, Remarks FROM CalibrationRecords ORDER BY CreatedAt DESC";
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

    public int GetCompletedTestCount()
    {
        using var conn = CreateConnection();
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM testmaster WHERE flag = '10000000'";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private string GetConfigValue(string defaultValue)
    {
        return defaultValue;
    }
}
