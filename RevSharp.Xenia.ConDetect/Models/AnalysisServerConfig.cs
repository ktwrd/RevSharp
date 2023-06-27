﻿using System.Text.Json;
using kate.shared.Helpers;
using RevSharp.Core;

namespace RevSharp.Xenia.Models.ContentDetection;

public class AnalysisServerConfig : BaseMongoModel
{
    public string TemplateId { get; set; }
    
    public string ServerId { get; set; }
    public string LogChannelId { get; set; }
    public string Guid { get; set; }
    
    public bool Enabled { get; set; }
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
        
        IgnoredChannelIds = Array.Empty<string>();
        IgnoredAuthorIds = Array.Empty<string>();
        
        DeleteThreshold = DefaultDeleteThreshold;
        FlagThreshold = DefaultFlagThreshold;
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
                JsonSerializer.Serialize(Analysis), Client.SerializerOptions);
            var thresholdDict = JsonSerializer.Deserialize<Dictionary<string, int>>(
                JsonSerializer.Serialize(Threshold, Client.SerializerOptions), Client.SerializerOptions);
            
            
            var dict = new Dictionary<string, decimal>();
            
            foreach (var key in validKeys)
            {
                var thresholdValue = thresholdDict[key];
                if (thresholdValue < 0)
                    continue;
                var analysisResult = decimal.Parse(analysisDict[key + "Average"].ToString());
                var things = Analysis.Annotations.Select(
                    (v) =>
                    {
                        var dct = JsonSerializer.Deserialize<Dictionary<string, object>>(
                            JsonSerializer.Serialize(v.Item1, Client.SerializerOptions), Client.SerializerOptions);
                        return ((dct[key]).ToString(), v.Item2);
                    }).ToArray();
                if (analysisResult >= thresholdValue)
                    dict.TryAdd(key, analysisResult);
            }

            return dict;
        }
    }
}
