using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using kate.shared.Helpers;

namespace RevSharp.Xenia.GoogleCloud.Perspective.Models;

public class AnalyzeCommentResponse
{
    /// <summary>
    /// A map from attribute name to per-attribute score objects. The attribute names will mirror the request's requestedAttributes.
    /// </summary>
    [JsonPropertyName("attributeScores")]
    public Dictionary<CommentAttributeName, AnalyzeCommentScore> AttributeScores { get; set; }
    /// <summary>
    /// Mirrors the request's languages. If no languages were specified, the API returns the auto-detected language.
    /// </summary>
    [JsonPropertyName("languages")]
    public string[] Languages { get; set; }
    /// <summary>
    /// Mirrors the request's clientToken.
    /// </summary>
    [JsonPropertyName("clientToken")]
    public string ClientToken { get; set; }
}

public class AnalyzeCommentRequest
{
    [JsonPropertyName("comment")]
    public AnalyzeCommentText Comment { get; set; }
    [JsonPropertyName("context")]
    public AnalyzeCommentRequestContext Context { get; set; }
    /// <summary>
    /// A map from attribute name to a configuration object. See the ‘Attributes and Languages’ page for a list of available attribute names. If no configuration options are specified, defaults are used, so the empty object {} is a valid (and common) choice. You can specify multiple attribute names here to get scores from multiple attributes in a single request.
    /// </summary>
    [JsonPropertyName("requestedAttributes")]
    public Dictionary<CommentAttributeName, AnalyzeCommentAttribute> RequestedAttributes { get; set; }
    /// <summary>
    /// A boolean value that indicates if the request should return spans that describe the scores for each part of the text (currently done at per-sentence level). Defaults to false.
    /// </summary>
    [JsonPropertyName("spanAnnotations")]
    public bool? SpanAnnotations { get; set; }
    /// <summary>
    /// A list of ISO 631-1 two-letter language codes specifying the language(s) that comment is in (for example, "en", "es", "fr", "de", etc). If unspecified, the API will auto-detect the comment language. If language detection fails, the API returns an error. Note: See currently supported languages on the ‘Attributes and Languages’ page. There is no simple way to use the API across languages with production support and languages with experimental support only.
    /// </summary>
    [JsonPropertyName("languages")]
    public string[]? Languages { get; set; }
    /// <summary>
    /// Whether the API is permitted to store comment and context from this request. Stored comments will be used for future research and community attribute building purposes to improve the API over time. Defaults to false (request data may be stored). Warning: This should be set to true if data being submitted is private (i.e. not publicly accessible), or if the data submitted contains content written by someone under 13 years old (or the relevant age determined by applicable law in my jurisdiction).
    /// </summary>
    [JsonPropertyName("doNotStore")]
    public bool? DoNotStore { get; set; }
    /// <summary>
    /// An opaque token that is echoed back in the response.
    /// </summary>
    [JsonPropertyName("clientToken")]
    public string? ClientToken { get; set; }
    /// <summary>
    /// An opaque session ID. This should be set for authorship experiences by the client side so that groups of requests can be grouped together into a session. This should not be used for any user-specific id. This is intended for abuse protection and individual sessions of interaction.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
    /// <summary>
    /// An opaque identifier associating this comment with a particular community within your platform. If set, this field allows us to differentiate comments from different communities, as each community may have different norms.
    /// </summary>
    [JsonPropertyName("communityId")]
    public string? CommunityId { get; set; }

    public AnalyzeCommentRequest()
    {
        SpanAnnotations = false;
        DoNotStore = false;
        Comment = new();
        RequestedAttributes = new();
    }

    public AnalyzeCommentRequest(string text)
        : this()
    {
        Comment = new AnalyzeCommentText();
        Comment.Text = text;
    }
    
    
    
    public AnalyzeCommentRequest WithText(string text)
    {
        Comment ??= new AnalyzeCommentText();
        Comment.Text = text;
        return this;
    }

    public AnalyzeCommentRequest AddLanguage(string lang)
    {
        Languages = Languages.ToArray().Concat(
            new string[]
            {
                lang
            }).ToArray();
        return this;
    }

    public AnalyzeCommentRequest AddAllAttrs()
    {
        foreach (var e in GeneralHelper.GetEnumList<CommentAttributeName>())
        {
            AddAttribute(e);
        }

        return this;
    }
    public AnalyzeCommentRequest AddAttribute(CommentAttributeName name, AnalyzeCommentAttributeType? type = null, float? threshold = null)
    {
        if (threshold != null && threshold < 0 || threshold > 1)
            throw new ArgumentException($"Parameter threshold must be >=0 or <=1");
        
        var instance = new AnalyzeCommentAttribute()
        {
            Type = type,
            Threshold = threshold
        };
        RequestedAttributes.TryAdd(name, instance);
        RequestedAttributes[name] = instance;
        return this;
    }
}

public class AnalyzeCommentRequestContext
{
    /// <summary>
    /// A list of objects providing the context for comment. The API currently does not make use of this field, but it may influence API responses in the future. 
    /// </summary>
    [JsonPropertyName("entries")]
    public CommentRequestContextItem[] Entries { get; set; }

    public AnalyzeCommentRequestContext()
    {
        Entries = Array.Empty<CommentRequestContextItem>();
    }
}

public class CommentRequestContextItem
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    public CommentRequestContextItem()
    {
        Text = null;
        Type = null;
    }
}

public enum ContextEntryType
{
    [EnumMember(Value = "PLAIN TEXT")]
    PlainText,
}

public class AnalyzeCommentAttribute
{
    /// <summary>
    /// The score type returned for this attribute. Currently, only "PROBABILITY" is supported. Probability scores are in the range [0,1].
    /// </summary>
    [JsonPropertyName("scoreType")]
    public AnalyzeCommentAttributeType? Type { get; set; }
    /// <summary>
    /// The API won't return scores that are below this threshold for this attribute. By default, all scores are returned.
    /// </summary>
    [JsonPropertyName("scoreThreshold")]
    public float? Threshold { get; set; }

    public AnalyzeCommentAttribute()
    {
        Type = AnalyzeCommentAttributeType.Probability;
    }
}

public enum AnalyzeCommentAttributeType
{
    [EnumMember(Value = "PROBABILITY")]
    Probability,
}

public class AnalyzeCommentText
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    public AnalyzeCommentText()
    {
        Text = "";
        Type = "PLAIN_TEXT";
    }
}

public enum AnalyzeCommentTextType
{
    [EnumMember(Value = "PLAIN_TEXT")]
    PlainText,
    [EnumMember(Value = "HTML")]
    HTML,
}

public class AnalyzeCommentScore
{
    [JsonPropertyName("summaryScore")]
    public AnalyzeScoreValue Summary { get; set; }
    /// <summary>
    /// A list of per-span scores for this attribute. These scores apply to different parts of the request's comment.text. Note: Some attributes may not return spanScores at all.
    /// </summary>
    [JsonPropertyName("spanScores")]
    public AnalyzeSpanScore[] SpanScores { get; set; }
}

public class AnalyzeScoreValue
{
    /// <summary>
    /// The attribute score for the span delimited by begin and end.
    /// </summary>
    [JsonPropertyName("value")]
    public float Value { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
}

public class AnalyzeSpanScore
{
    /// <summary>
    /// Beginning character index of the text span in the request comment.
    /// </summary>
    [JsonPropertyName("begin")]
    public int Begin { get; set; }
    /// <summary>
    /// End of the text span in the request comment.
    /// </summary>
    [JsonPropertyName("end")]
    public int Ends { get; set; }
    [JsonPropertyName("score")]
    public AnalyzeScoreValue Score { get; set; }
}

public enum CommentAttributeName
{
    [EnumMember(Value = "TOXICITY")]
    TOXICITY,
    [EnumMember(Value = "SEVERE_TOXICITY")]
    SEVERE_TOXICITY,
    [EnumMember(Value = "IDENTITY_ATTACK")]
    IDENTITY_ATTACK,
    [EnumMember(Value = "INSULT")]
    INSULT,
    [EnumMember(Value = "PROFANITY")]
    PROFANITY,
    [EnumMember(Value = "THREAT")]
    THREAT,
}