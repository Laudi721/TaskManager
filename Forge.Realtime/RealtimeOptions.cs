namespace Forge.Realtime;

public class RealtimeOptions
{
    public const string SectionName = "Realtime";

    /// <summary>
    /// StackExchange.Redis connection string. When null/empty, SignalR runs without backplane
    /// (single-instance mode, no cross-API propagation).
    /// </summary>
    public string? Redis { get; set; }

    /// <summary>
    /// Channel prefix shared by all Forge backends connected to the same Redis instance.
    /// Both desk.webapi and production.webapi MUST use the same value, otherwise messages
    /// will not cross between them.
    /// </summary>
    public string ChannelPrefix { get; set; } = "forge";

    /// <summary>
    /// Endpoint where the hub is mapped. Clients connect to "{ApiBaseUrl}{HubPath}".
    /// </summary>
    public string HubPath { get; set; } = "/hubs/notifications";
}
