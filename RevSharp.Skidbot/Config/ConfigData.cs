﻿namespace RevSharp.Skidbot;

/// <summary>
/// Content of the config that is stored at <see cref="FeatureFlags.ConfigLocation"/>
/// </summary>
public class ConfigData
{
    /// <summary>
    /// Token to use for Revolt
    /// </summary>
    public string Token { get; set; }
    /// <summary>
    /// Is the token provided for a bot?
    /// </summary>
    public bool IsBot { get; set; }
    /// <summary>
    /// Command prefix
    /// </summary>
    public string Prefix { get; set; }
    /// <summary>
    /// Connection Url for the MongoDb Driver
    /// </summary>
    public string MongoConnectionUrl { get; set; }
    /// <summary>
    /// Name of the database to store everything in
    /// </summary>
    public string MongoDatabaseName { get; set; }
    /// <summary>
    /// Google Cloud API Credentials
    /// </summary>
    public GCSConfig GoogleCloud { get; set; }
    /// <summary>
    /// Name of the GCS Bucket where all uploaded media will go to.
    /// </summary>
    public string ContentDetectionBucket { get; set; }
    /// <summary>
    /// User Ids who are marked as an owner.
    /// </summary>
    public string[] OwnerUserIds { get; set; }
    /// <summary>
    /// Authentik Base Url for <see cref="Modules.AuthentikModule"/>
    /// </summary>
    public string AuthentikUrl { get; set; }
    /// <summary>
    /// Token for <see cref="Modules.AuthentikModule"/>
    /// </summary>
    public string AuthentikToken { get; set; }
    /// <summary>
    /// Port for Prometheus exporter to listen on
    /// </summary>
    public int PrometheusPort { get; set; }
    /// <summary>
    /// Enable <see cref="Controllers.StatisticController"/>
    /// </summary>
    public bool PrometheusEnable { get; set; }

    public ConfigData()
    {
        Token = "";
        IsBot = false;
        Prefix = "r.";
        MongoConnectionUrl = "mongodb://user:password@localhost:27021";
        MongoDatabaseName = "skidbot_revolt";
        GoogleCloud = new GCSConfig();
        ContentDetectionBucket = "skidbot-condetect-data";
        OwnerUserIds = Array.Empty<string>();
        AuthentikUrl = "";
        AuthentikToken = "";
        PrometheusPort = 8771;
        PrometheusEnable = true;
    }
}