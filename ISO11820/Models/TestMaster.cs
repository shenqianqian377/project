namespace ISO11820.Models;

public class TestMaster
{
    public string ProductId { get; set; } = "";
    public string TestId { get; set; } = "";
    public DateTime TestDate { get; set; }
    public double AmbTemp { get; set; }
    public double AmbHumi { get; set; }
    public string According { get; set; } = "ISO 11820:2022";
    public string Operator { get; set; } = "";
    public string ApparatusId { get; set; } = "";
    public string ApparatusName { get; set; } = "";
    public DateTime ApparatusChkDate { get; set; }
    public string RptNo { get; set; } = "";
    public double PreWeight { get; set; }
    public double PostWeight { get; set; }
    public double LostWeight { get; set; }
    public double LostWeightPer { get; set; }
    public int TotalTestTime { get; set; }
    public int ConstPower { get; set; }
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
    public double DeltaTf1 { get; set; }
    public double DeltaTf2 { get; set; }
    public double DeltaTf { get; set; }
    public double DeltaTs { get; set; }
    public double DeltaTc { get; set; }
    public string Memo { get; set; } = "";
    public string Flag { get; set; } = "";
    public int DurationMode { get; set; }
    public int TargetDuration { get; set; }
}