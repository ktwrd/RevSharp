namespace RevSharp.Core;

internal partial class EndpointFactory
{
    internal string SyncUnreads() => $"{BaseUrl}/sync/unreads";
    internal string SyncSettingsSet(long timestamp) => $"{BaseUrl}/sync/settings/set?timestamp={timestamp}";
    internal string SyncSettingsFetch() => $"{BaseUrl}/sync/settings/fetch";
}