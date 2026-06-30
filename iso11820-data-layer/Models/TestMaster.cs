namespace ISO11820.Models;

public class TestMaster
{
    public string ProductId { get; set; } = "";
    public string TestId { get; set; } = "";
    public string TestDate { get; set; } = "";
    public string Operator { get; set; } = "";
    public double AmbTemp { get; set; }
    public double AmbHumi { get; set; }
    public string According { get; set; } = "ISO 11820:2022";
    public string ApparatusId { get; set; } = "FURNACE-01";
    public string ApparatusName { get; set; } = "一号试验炉";
    public string ApparatusChkDate { get; set; } = "";
    public string RptNo { get; set; } = "";
    public double PreWeight { get; set; }
    public double PostWeight { get; set; }
    public double LostWeight { get; set; }
    public double LostWeightPer { get; set; }
    public int TotalTestTime { get; set; }
    public double ConstPower { get; set; }
    public string PhenoCode { get; set; } = "";
    public int FlameTime { get; set; }
    public int FlameDuration { get; set; }
    public double MaxTf1 { get; set; }
    public double MaxTf2 { get; set; }
    public double MaxTs { get; set; }
    public double MaxTc { get; set; }
    public int MaxTf1Time { get; set; }
    public int MaxTf2Time { get; set; }
    public int MaxTsTime { get; set; }
    public int MaxTcTime { get; set; }
    public double FinalTf1 { get; set; }
    public double FinalTf2 { get; set; }
    public double FinalTs { get; set; }
    public double FinalTc { get; set; }
    public int FinalTf1Time { get; set; }
    public int FinalTf2Time { get; set; }
    public int FinalTsTime { get; set; }
    public int FinalTcTime { get; set; }
    public double DeltaTf1 { get; set; }
    public double DeltaTf2 { get; set; }
    public double DeltaTf { get; set; }
    public double DeltaTs { get; set; }
    public double DeltaTc { get; set; }
    public string Flag { get; set; } = "";
    public int DurationMode { get; set; }
    public int TargetDuration { get; set; }
}
