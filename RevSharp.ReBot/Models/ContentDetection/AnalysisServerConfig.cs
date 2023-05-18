using System.Text.Json;
using kate.shared.Helpers;

namespace RevSharp.ReBot.Models.ContentDetection;

public class AnalysisServerConfig : BaseMongoModel
{
    public string TemplateId { get; set; }
    
    public string ServerId { get; set; }
    public string LogChannelId { get; set; }

    public static ConfigThreshold DefaultDeleteThreshold =>
        new()
        {
            Adult = 5,
            Spoof = 5,
            Medical = -1,
            Violence = 4,
            Racy = -1
        };

    public static ConfigThreshold DefaultFlagThreshold =>
        new()
        {
            Adult = 2,
            Spoof = 3,
            Medical = -1,
            Violence = 3,
            Racy = 2
        };
    public ConfigThreshold DeleteThreshold { get; set; }
    public ConfigThreshold FlagThreshold { get; set; }

    public AnalysisServerConfig()
    {
        TemplateId = GeneralHelper.GenerateUID();
        DeleteThreshold = DefaultDeleteThreshold;
        FlagThreshold = DefaultFlagThreshold;
    }
    
    #region Methods for matching
    public ContentAnalysisMessageMatch GetMessageThresholdMatch(AnalysisResult analysisResult,
        ConfigThreshold threshold)
    {
        return new ContentAnalysisMessageMatch()
        {
            Adult = analysisResult.AdultAverage >= threshold.Adult,
            Spoof = analysisResult.SpoofAverage >= threshold.Spoof,
            Medical = analysisResult.MedicalAverage >= threshold.Medical,
            Violence = analysisResult.ViolenceAverage >= threshold.Violence,
            Racy = analysisResult.RacyAverage >= threshold.Racy,
        };
    }

    public string? HasThresholdTrigger(AnalysisResult analysisResult,
        ConfigThreshold threshold)
    {
        var match = GetMessageThresholdMatch(analysisResult, threshold);
        return match.Total < 1 ?
            null :
            match.Majority;
    }
    #endregion
}

public class ContentAnalysisMessageMatch
{
    public bool Adult { get; init; }
    public bool Spoof { get; init; }
    public bool Medical { get; init; }
    public bool Violence { get; init; }
    public bool Racy { get; init; }

    public int Total
        => new bool[]
        {
            Adult,
            Spoof,
            Medical,
            Violence,
            Racy
        }.Select(v => v ? 1 : 0).Sum();

    public string? Majority
    {
        get
        {
            var selfSer = JsonSerializer.Serialize(this, Program.SerializerOptions);
            var selfDict = JsonSerializer.Deserialize<Dictionary<string, object>>(selfSer, Program.SerializerOptions)
                           ?? new Dictionary<string, object>();
            foreach (var pair in selfDict)
            {
                if (pair.Key is "Total" or "Majority")
                    continue;

                if (pair.Value.ToString() == "true")
                    return pair.Key.ToString();
            }

            return null;
        }
    }
}
