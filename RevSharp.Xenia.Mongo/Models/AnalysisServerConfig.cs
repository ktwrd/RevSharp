using System.Text.Json;
using System.Text.Json.Serialization;
using kate.shared.Helpers;
using RevSharp.Core;
using RevSharp.Xenia.GoogleCloud.Perspective.Models;

namespace RevSharp.Xenia.Models.ContentDetection;

public class AnalysisServerConfig : BaseMongoModel
{
    public const string CollectionName = "contentDetectionServerConfig";
    public string TemplateId { get; set; }
    
    public string ServerId { get; set; }
    public string LogChannelId { get; set; }
    public string Guid { get; set; }
    
    public bool Enabled { get; set; }
    /// <summary>
    /// Enable text detection on messages with Perspective. Assumes poster is >=13yo
    ///
    /// AnalyzeMessage->AnalyzeMessageText
    /// </summary>
    public bool AllowTextDetection { get; set; }
    /// <summary>
    /// Enable text detection with Perspective on OCR Detected Text. Assumes poster is >=13yo
    ///
    /// AnalyzeMessage->GetMedia->GetMediaTextWithOCR->AnalyzeMessageText
    /// </summary>
    public bool AllowMediaTextDetection { get; set; }
    public bool AllowAnalysis { get; set; }
    public bool HasRequested { get; set; }
    public bool IsBanned { get; set; }
    public string? BanReason { get; set; }
    public string[] IgnoredChannelIds { get; set; }
    public string[] IgnoredAuthorIds { get; set; }
    
    public ConfigThreshold DeleteThreshold { get; set; }
    public ConfigThreshold FlagThreshold { get; set; }
    public AnalysisServerConfig()
    {
        TemplateId = GeneralHelper.GenerateUID();

        ServerId = "";
        LogChannelId = "";
        Guid = new Guid().ToString();

        Enabled = false;
        AllowAnalysis = false;
        HasRequested = false;
        IsBanned = false;
        BanReason = null;

        AllowTextDetection = false;
        AllowMediaTextDetection = false;
        
        IgnoredChannelIds = Array.Empty<string>();
        IgnoredAuthorIds = Array.Empty<string>();
        
        DeleteThreshold = DefaultDeleteThreshold;
        FlagThreshold = DefaultFlagThreshold;
        TextDeleteThreshold = DefaultTextDeleteThreshold;
        TextFlagThreshold = DefaultTextFlagThreshold;
    }
    public bool ShouldAllowAnalysis()
    {
        if (IsBanned)
            return false;

        return AllowAnalysis && Enabled;
    }

    public static ConfigThreshold DefaultDeleteThreshold =>
        new()
        {
            Adult = 5,
            Spoof = -1,
            Medical = -1,
            Violence = 5,
            Racy = -1
        };

    public static ConfigThreshold DefaultFlagThreshold =>
        new()
        {
            Adult = 3,
            Spoof = -1,
            Medical = -1,
            Violence = 4,
            Racy = -1
        };

    public Dictionary<string, float> TextDeleteThreshold { get; set; }
    public Dictionary<string, float> TextFlagThreshold { get; set; }
    public static Dictionary<string, float> DefaultTextDeleteThreshold =>
        new()
        {
            { CommentAttributeName.TOXICITY.ToString(), 0.95f },
            { CommentAttributeName.SEVERE_TOXICITY.ToString(), 0.90f },
            { CommentAttributeName.IDENTITY_ATTACK.ToString(), 0.85f },
            { CommentAttributeName.INSULT.ToString(), 0.5f },
            { CommentAttributeName.PROFANITY.ToString(), 0.90f },
            { CommentAttributeName.THREAT.ToString(), 0.85f }
        };
    public static Dictionary<string, float> DefaultTextFlagThreshold =>
        new()
        {
            { CommentAttributeName.TOXICITY.ToString(), 0.75f },
            { CommentAttributeName.SEVERE_TOXICITY.ToString(), 0.7f },
            { CommentAttributeName.IDENTITY_ATTACK.ToString(), 0.6f },
            { CommentAttributeName.INSULT.ToString(), 0.4f },
            { CommentAttributeName.PROFANITY.ToString(), 0.7f },
            { CommentAttributeName.THREAT.ToString(), 0.7f }
        };
    
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
            Analysis = analysisResult,
            Threshold = threshold
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
    public ConfigThreshold Threshold { get; init; }
    public AnalysisResult Analysis { get; init; }

    public ContentAnalysisMessageMatch()
    {
        Threshold = new ConfigThreshold();
        Analysis = new AnalysisResult();
    }
    public string? Majority =>
        MajorityItems.FirstOrDefault();

    public string[] MajorityItems
    {
        get
        {
            return MajorityPairs
                .Select(v => (v.Key, v.Value))
                .OrderBy(v => v.Value)
                .Select(v => v.Key)
                .ToArray();

        }
    }

    public Dictionary<string, decimal> MajorityPairs
    {
        get
        {
            var validKeys = new string[]
            {
                "Adult", "Spoof", "Medical", "Violence", "Racy"
            };
            var analysisDict = JsonSerializer.Deserialize<Dictionary<string, object>>(
                JsonSerializer.Serialize(Analysis), Client.SerializerOptions) ?? new Dictionary<string, object>();
            var thresholdDict = JsonSerializer.Deserialize<Dictionary<string, int>>(
                JsonSerializer.Serialize(Threshold, Client.SerializerOptions), Client.SerializerOptions) ?? new Dictionary<string, int>();
            
            
            var dict = new Dictionary<string, decimal>();
            
            foreach (var key in validKeys)
            {
                var thresholdValue = thresholdDict[key];
                if (thresholdValue < 0)
                    continue;
                var analysisResult = decimal.Parse(analysisDict[key + "Average"].ToString() ?? string.Empty);
                var things = Analysis.Annotations.Select(
                    (v) =>
                    {
                        var dct = JsonSerializer.Deserialize<Dictionary<string, object>>(
                            JsonSerializer.Serialize(v.Item1, Client.SerializerOptions), Client.SerializerOptions);
                        return (dct?[key].ToString(), v.Item2);
                    }).ToArray();
                if (analysisResult >= thresholdValue)
                    dict.TryAdd(key, analysisResult);
            }

            return dict;
        }
    }
}
